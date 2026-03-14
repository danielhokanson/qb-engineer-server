using Microsoft.AspNetCore.Identity;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Context;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Initials { get; set; }
    public string? AvatarColor { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Setup token flow (admin creates user → employee completes account)
    public string? SetupToken { get; set; }
    public DateTime? SetupTokenExpiresAt { get; set; }

    // PIN for kiosk auth (Tier 2: barcode + PIN)
    public string? PinHash { get; set; }

    // Barcode for kiosk scan
    public string? EmployeeBarcode { get; set; }

    // Team assignment
    public int? TeamId { get; set; }

    // Work location (drives state withholding)
    public int? WorkLocationId { get; set; }
    public CompanyLocation? WorkLocation { get; set; }

    // Accounting integration (QB Employee ID for time activity sync)
    public string? AccountingEmployeeId { get; set; }

    // SSO identity linking
    public string? GoogleId { get; set; }
    public string? MicrosoftId { get; set; }
    public string? OidcSubjectId { get; set; }
    public string? OidcProvider { get; set; }
}
