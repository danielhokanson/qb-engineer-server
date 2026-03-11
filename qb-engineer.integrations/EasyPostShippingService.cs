using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using EasyPost;
using EasyPost.Models.API;
using EasyPost.Parameters;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class EasyPostShippingService : IShippingService
{
    private readonly Client _client;
    private readonly ILogger<EasyPostShippingService> _logger;

    public EasyPostShippingService(IOptions<EasyPostOptions> options, ILogger<EasyPostShippingService> logger)
    {
        _logger = logger;
        var opts = options.Value;
        _client = new Client(new ClientConfiguration(opts.ApiKey));
    }

    public async Task<List<ShippingRate>> GetRatesAsync(ShipmentRequest request, CancellationToken ct)
    {
        _logger.LogInformation("EasyPost GetRates for {PackageCount} package(s) to {City}, {State}",
            request.Packages.Count, request.ToAddress.City, request.ToAddress.State);

        var results = new List<ShippingRate>();

        foreach (var pkg in request.Packages)
        {
            var shipmentParams = new EasyPost.Parameters.Shipment.Create
            {
                FromAddress = BuildAddressParams(request.FromAddress),
                ToAddress = BuildAddressParams(request.ToAddress),
                Parcel = new EasyPost.Parameters.Parcel.Create
                {
                    Weight = (double)pkg.WeightLbs * 16, // EasyPost expects ounces
                    Length = (double)pkg.LengthIn,
                    Width = (double)pkg.WidthIn,
                    Height = (double)pkg.HeightIn,
                },
            };

            var shipment = await _client.Shipment.Create(shipmentParams);

            if (shipment.Rates != null)
            {
                foreach (var rate in shipment.Rates)
                {
                    var carrierId = $"{rate.Carrier?.ToLowerInvariant()}-{rate.Service?.ToLowerInvariant()}";
                    var price = decimal.TryParse(rate.Price, out var p) ? p : 0m;
                    var days = rate.DeliveryDays ?? 0;

                    results.Add(new ShippingRate(
                        carrierId,
                        rate.Carrier ?? "Unknown",
                        rate.Service ?? "Unknown",
                        price,
                        days));
                }
            }
        }

        _logger.LogInformation("EasyPost returned {RateCount} rate(s)", results.Count);
        return results;
    }

    public async Task<ShippingLabel> CreateLabelAsync(ShipmentRequest request, string carrierId, CancellationToken ct)
    {
        _logger.LogInformation("EasyPost CreateLabel with carrier {CarrierId}", carrierId);

        // Use the first package for label creation
        var pkg = request.Packages.FirstOrDefault()
            ?? throw new InvalidOperationException("At least one package is required to create a shipping label");

        var shipmentParams = new EasyPost.Parameters.Shipment.Create
        {
            FromAddress = BuildAddressParams(request.FromAddress),
            ToAddress = BuildAddressParams(request.ToAddress),
            Parcel = new EasyPost.Parameters.Parcel.Create
            {
                Weight = (double)pkg.WeightLbs * 16,
                Length = (double)pkg.LengthIn,
                Width = (double)pkg.WidthIn,
                Height = (double)pkg.HeightIn,
            },
        };

        var shipment = await _client.Shipment.Create(shipmentParams);

        // Find the matching rate by carrier-service ID
        var parts = carrierId.Split('-', 2);
        var matchedRate = shipment.Rates?.FirstOrDefault(r =>
            string.Equals(r.Carrier, parts[0], StringComparison.OrdinalIgnoreCase) &&
            (parts.Length < 2 || string.Equals(r.Service, parts[1], StringComparison.OrdinalIgnoreCase)));

        matchedRate ??= shipment.LowestRate();

        if (matchedRate == null)
            throw new InvalidOperationException($"No matching rate found for carrier '{carrierId}'");

        var purchased = await _client.Shipment.Buy(shipment.Id!, matchedRate);

        var trackingNumber = purchased.TrackingCode ?? string.Empty;
        var labelUrl = purchased.PostageLabel?.LabelUrl ?? string.Empty;
        var carrierName = matchedRate.Carrier ?? "Unknown";

        _logger.LogInformation("EasyPost label created — tracking {Tracking}, carrier {Carrier}",
            trackingNumber, carrierName);

        return new ShippingLabel(trackingNumber, labelUrl, carrierName);
    }

    public async Task<ShipmentTracking?> GetTrackingAsync(string trackingNumber, CancellationToken ct)
    {
        _logger.LogInformation("EasyPost GetTracking({TrackingNumber})", trackingNumber);

        try
        {
            var tracker = await _client.Tracker.Create(new EasyPost.Parameters.Tracker.Create
            {
                TrackingCode = trackingNumber,
            });

            var events = tracker.TrackingDetails?
                .OrderByDescending(d => d.Datetime)
                .Select(d => new TrackingEvent(
                    d.Datetime ?? DateTime.UtcNow,
                    d.TrackingLocation?.City ?? "Unknown",
                    d.Message ?? d.Status ?? "Unknown"))
                .ToList() ?? [];

            return new ShipmentTracking(
                trackingNumber,
                tracker.Status ?? "Unknown",
                tracker.EstDeliveryDate,
                events);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EasyPost tracking lookup failed for {TrackingNumber}", trackingNumber);
            return null;
        }
    }

    public async Task<AddressValidationResponseModel> ValidateAddressAsync(ShippingAddress address, CancellationToken ct)
    {
        _logger.LogInformation("EasyPost ValidateAddress for {City}, {State} {Zip}",
            address.City, address.State, address.Zip);

        try
        {
            var addressParams = new EasyPost.Parameters.Address.Create
            {
                Street1 = address.Street,
                City = address.City,
                State = address.State,
                Zip = address.Zip,
                Country = address.Country,
                Verify = true,
            };

            var verified = await _client.Address.Create(addressParams);

            var messages = verified.Verifications?.Delivery?.Errors?
                .Select(e => e.Message ?? "Unknown error")
                .ToList() ?? [];

            var isValid = verified.Verifications?.Delivery?.Success == true;

            return new AddressValidationResponseModel(
                isValid,
                verified.Street1,
                verified.City,
                verified.State,
                verified.Zip,
                verified.Country,
                messages);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EasyPost address validation failed");
            return new AddressValidationResponseModel(
                false, null, null, null, null, null,
                [ex.Message]);
        }
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct)
    {
        try
        {
            // Validate a known address as a connectivity test
            var testAddress = new EasyPost.Parameters.Address.Create
            {
                Street1 = "417 Montgomery Street",
                City = "San Francisco",
                State = "CA",
                Zip = "94104",
                Country = "US",
            };

            await _client.Address.Create(testAddress);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EasyPost connection test failed");
            return false;
        }
    }

    private static EasyPost.Parameters.Address.Create BuildAddressParams(ShippingAddress address)
    {
        return new EasyPost.Parameters.Address.Create
        {
            Name = address.Name,
            Street1 = address.Street,
            City = address.City,
            State = address.State,
            Zip = address.Zip,
            Country = address.Country,
        };
    }
}
