using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IShippingService
{
    Task<List<ShippingRate>> GetRatesAsync(ShipmentRequest request, CancellationToken ct);
    Task<ShippingLabel> CreateLabelAsync(ShipmentRequest request, string carrierId, CancellationToken ct);
    Task<ShipmentTracking?> GetTrackingAsync(string trackingNumber, CancellationToken ct);
    Task<AddressValidationResponseModel> ValidateAddressAsync(ShippingAddress address, CancellationToken ct);
    Task<bool> TestConnectionAsync(CancellationToken ct);
}
