using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Data.Context;
using Serilog;

namespace QBEngineer.Api.Data;

public static partial class SeedData
{
    private static async Task SeedSalesTaxRatesAsync(AppDbContext db)
    {
        if (await db.SalesTaxRates.AnyAsync()) return;

        var epoch = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // US state-level base rates (2024/2025). Rates are the STATE base rate only.
        // Local (county/city) rates add on top — admins should adjust to their effective
        // combined rate for each nexus jurisdiction.
        // Sources: Tax Foundation 2024 State Individual Income Tax Rates and Sales Tax Handbook.
        // Zero-rate states (OR, MT, NH, DE, AK) are included for completeness so the system
        // returns 0% automatically rather than falling back to the default.
        var states = new (string Code, string Name, decimal Rate)[]
        {
            ("AL", "Alabama",        0.0400m),
            ("AK", "Alaska",         0.0000m), // No state tax; local rates apply
            ("AZ", "Arizona",        0.0560m),
            ("AR", "Arkansas",       0.0650m),
            ("CA", "California",     0.0725m),
            ("CO", "Colorado",       0.0290m),
            ("CT", "Connecticut",    0.0635m),
            ("DE", "Delaware",       0.0000m), // No sales tax
            ("FL", "Florida",        0.0600m),
            ("GA", "Georgia",        0.0400m),
            ("HI", "Hawaii",         0.0400m), // General Excise Tax
            ("ID", "Idaho",          0.0600m),
            ("IL", "Illinois",       0.0625m),
            ("IN", "Indiana",        0.0700m),
            ("IA", "Iowa",           0.0600m),
            ("KS", "Kansas",         0.0650m),
            ("KY", "Kentucky",       0.0600m),
            ("LA", "Louisiana",      0.0445m),
            ("ME", "Maine",          0.0550m),
            ("MD", "Maryland",       0.0600m),
            ("MA", "Massachusetts",  0.0625m),
            ("MI", "Michigan",       0.0600m),
            ("MN", "Minnesota",      0.0688m),
            ("MS", "Mississippi",    0.0700m),
            ("MO", "Missouri",       0.0423m),
            ("MT", "Montana",        0.0000m), // No sales tax
            ("NE", "Nebraska",       0.0550m),
            ("NV", "Nevada",         0.0685m),
            ("NH", "New Hampshire",  0.0000m), // No sales tax
            ("NJ", "New Jersey",     0.0663m),
            ("NM", "New Mexico",     0.0500m), // Gross Receipts Tax
            ("NY", "New York",       0.0400m),
            ("NC", "North Carolina", 0.0475m),
            ("ND", "North Dakota",   0.0500m),
            ("OH", "Ohio",           0.0575m),
            ("OK", "Oklahoma",       0.0450m),
            ("OR", "Oregon",         0.0000m), // No sales tax
            ("PA", "Pennsylvania",   0.0600m),
            ("RI", "Rhode Island",   0.0700m),
            ("SC", "South Carolina", 0.0600m),
            ("SD", "South Dakota",   0.0420m),
            ("TN", "Tennessee",      0.0700m),
            ("TX", "Texas",          0.0625m),
            ("UT", "Utah",           0.0610m),
            ("VT", "Vermont",        0.0600m),
            ("VA", "Virginia",       0.0530m),
            ("WA", "Washington",     0.0650m),
            ("WV", "West Virginia",  0.0600m),
            ("WI", "Wisconsin",      0.0500m),
            ("WY", "Wyoming",        0.0400m),
        };

        var rates = states.Select(s => new SalesTaxRate
        {
            Name = $"{s.Name} Sales Tax",
            Code = s.Code,
            StateCode = s.Code,
            Rate = s.Rate,
            EffectiveFrom = epoch,
            EffectiveTo = null,
            IsDefault = false,
            IsActive = true,
            Description = s.Rate == 0
                ? $"{s.Name}: no state sales tax. Verify if local rates apply."
                : $"{s.Name} state base rate. Add local rates for your nexus jurisdictions.",
        }).ToList();

        // Mark a general default (0%) — overridden once the admin configures their state
        rates.Add(new SalesTaxRate
        {
            Name = "Default (No Tax)",
            Code = "DEFAULT",
            StateCode = null,
            Rate = 0.0000m,
            EffectiveFrom = epoch,
            EffectiveTo = null,
            IsDefault = true,
            IsActive = true,
            Description = "Fallback rate when no state-specific rate is found. Update to your default jurisdiction rate.",
        });

        db.SalesTaxRates.AddRange(rates);
        await db.SaveChangesAsync();
        Log.Information("Seeded {Count} sales tax rates (50 states + default)", rates.Count);
    }
}
