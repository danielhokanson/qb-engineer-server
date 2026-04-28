namespace QBEngineer.Api.Capabilities.Discovery;

/// <summary>
/// Phase 4 Phase-F — Authoritative static catalog of the 8 presets from
/// Phase 4B preset-design.md. The recommendation engine routes to one of
/// the 7 named presets; PRESET-CUSTOM is the explicit opt-out.
///
/// Each preset's <see cref="PresetDefinition.EnabledCapabilities"/> is
/// computed from the 4B per-preset spec by:
///   1. Starting from the catalog's 41 default-on entries
///   2. Removing entries the preset explicitly disables (e.g. PRESET-03
///      Distribution disables BOM/ROUTING/MFG-*).
///   3. Adding entries the preset explicitly enables (per 4B "explicitly
///      enabled" markers).
///
/// PRESET-CUSTOM is empty — apply-time logic substitutes the 41 catalog
/// defaults (per 4B Open Question 5 / 4F-decisions-log).
/// </summary>
public static class PresetCatalog
{
    /// <summary>The 41 default-on capabilities from the 4A catalog. Used as a baseline for preset assembly.</summary>
    private static readonly string[] CatalogDefaultsBaseline =
    [
        "CAP-IDEN-AUTH-PASSWORD", "CAP-IDEN-USERS", "CAP-IDEN-ROLES",
        "CAP-IDEN-TENANT-CONFIG", "CAP-IDEN-AUDIT-SYSTEM-LOG",
        "CAP-MD-CUSTOMERS", "CAP-MD-VENDORS", "CAP-MD-PARTS", "CAP-MD-BOM",
        "CAP-MD-ROUTING", "CAP-MD-WORKCENTERS", "CAP-MD-LOCATIONS",
        "CAP-MD-CALENDARS", "CAP-MD-EMPLOYEES", "CAP-MD-ASSETS", "CAP-MD-UOM",
        "CAP-MD-CURRENCIES", "CAP-MD-TAXCODES",
        "CAP-P2P-PO", "CAP-P2P-RECEIVE",
        "CAP-O2C-QUOTE", "CAP-O2C-SO", "CAP-O2C-PICKPACK", "CAP-O2C-SHIP",
        "CAP-O2C-INVOICE", "CAP-O2C-CASH",
        "CAP-MFG-WO-RELEASE", "CAP-MFG-MATL-ISSUE", "CAP-MFG-LABOR",
        "CAP-MFG-MULTIOP", "CAP-MFG-COMPLETE", "CAP-MFG-SHOPFLOOR",
        "CAP-INV-CORE", "CAP-INV-CYCLECOUNT",
        "CAP-MAINT-ASSETLIFECYCLE",
        "CAP-ACCT-BUILTIN", "CAP-ACCT-EXPENSES",
        "CAP-HR-HIRE", "CAP-HR-TERMINATION", "CAP-HR-TIMETRACK",
        "CAP-RPT-OPERATIONAL", "CAP-RPT-INVVAL", "CAP-RPT-DASHBOARDS",
        "CAP-CROSS-PERMS-MATRIX", "CAP-CROSS-ACTIVITY-LOG", "CAP-CROSS-LIST-UX",
        "CAP-CROSS-BULK-OPS", "CAP-CROSS-DOCS", "CAP-CROSS-ATTACHMENTS",
        "CAP-CROSS-NOTIFICATIONS", "CAP-CROSS-INTEG-FILE", "CAP-CROSS-CONCURRENCY",
        "CAP-EXT-KANBAN", "CAP-EXT-MOBILE",
        "CAP-IDEN-CAPABILITY-ADMIN",
    ];

    private static IReadOnlyList<string> AssemblePreset(
        IEnumerable<string> remove,
        IEnumerable<string> add)
    {
        var set = new HashSet<string>(CatalogDefaultsBaseline, StringComparer.Ordinal);
        foreach (var r in remove) set.Remove(r);
        foreach (var a in add) set.Add(a);
        return set.OrderBy(c => c, StringComparer.Ordinal).ToList();
    }

    public static PresetDefinition Preset01_TwoPersonShop { get; } = new(
        Id: "PRESET-01",
        Name: "Two-Person Shop",
        ShortDescription: "An owner-operator or two-person business doing light manufacturing or trade work, with built-in lightweight accounting and minimum ceremony.",
        TargetProfile: "1–3 people, single product line or low-volume job shop, no inspection requirements, single location.",
        EnabledCapabilities: AssemblePreset(
            // Disabled vs catalog defaults (per 4B PRESET-01)
            remove: [],
            // Explicitly enabled
            add: []));

    public static PresetDefinition Preset02_GrowingJobShop { get; } = new(
        Id: "PRESET-02",
        Name: "Growing Job Shop",
        ShortDescription: "Small-to-mid manufacturer with shop-floor / office split, external accounting (QuickBooks), light planning, and shop-floor kiosk auth.",
        TargetProfile: "4–25 people, 2–6 work centers, mixed repeat and one-off jobs, no regulated industry.",
        EnabledCapabilities: AssemblePreset(
            remove: ["CAP-ACCT-BUILTIN"],
            add:
            [
                "CAP-ACCT-EXTERNAL",
                "CAP-MD-PRICELIST",
                "CAP-P2P-RFQ", "CAP-P2P-SUBCONTRACT",
                "CAP-O2C-RMA", "CAP-O2C-COLLECTIONS",
                "CAP-PLAN-SAFETYSTOCK",
                "CAP-INV-RESERVE",
                "CAP-IDEN-AUTH-KIOSK", "CAP-EXT-SHOPFLOOR-KIOSK",
                "CAP-HR-LEAVE",
            ]));

    public static PresetDefinition Preset03_Distribution { get; } = new(
        Id: "PRESET-03",
        Name: "Distribution / Wholesale",
        ShortDescription: "Pure-resell or assembly-light business — buying, storing, picking, shipping. Heavy inventory and procurement automation, no production.",
        TargetProfile: "5–50 people, 50–500+ SKUs, drop-ship and back-to-back patterns, often industrial supply / food / beverage.",
        EnabledCapabilities: AssemblePreset(
            remove:
            [
                "CAP-MD-BOM", "CAP-MD-ROUTING", "CAP-MD-WORKCENTERS", "CAP-MD-CALENDARS",
                "CAP-MFG-WO-RELEASE", "CAP-MFG-MATL-ISSUE", "CAP-MFG-LABOR",
                "CAP-MFG-MULTIOP", "CAP-MFG-COMPLETE", "CAP-MFG-SHOPFLOOR",
                "CAP-ACCT-BUILTIN",
                "CAP-EXT-KANBAN",
            ],
            add:
            [
                "CAP-ACCT-EXTERNAL",
                "CAP-MD-PRICELIST",
                "CAP-P2P-RFQ", "CAP-P2P-DROPSHIP", "CAP-P2P-BACKTOBACK",
                "CAP-O2C-LEAD", "CAP-O2C-COLLECTIONS", "CAP-O2C-RMA", "CAP-O2C-CREDITMEMO",
                "CAP-PLAN-SAFETYSTOCK", "CAP-PLAN-FORECAST",
                "CAP-INV-RESERVE", "CAP-INV-PICKWAVE",
                "CAP-HR-LEAVE", "CAP-HR-SHIFTS",
            ]));

    public static PresetDefinition Preset04_ProductionManufacturer { get; } = new(
        Id: "PRESET-04",
        Name: "Production Manufacturer",
        ShortDescription: "A real manufacturer with dedicated production manager, formal routings, multi-step shop-floor, variance review, and ISO 9001 baseline quality.",
        TargetProfile: "25–200 people, 5–50 work centers, dedicated production / buyer / quality roles.",
        EnabledCapabilities: AssemblePreset(
            remove: ["CAP-ACCT-BUILTIN"],
            add:
            [
                "CAP-ACCT-EXTERNAL",
                "CAP-MD-PRICELIST", "CAP-MD-ECO",
                "CAP-P2P-RFQ", "CAP-P2P-SUBCONTRACT", "CAP-P2P-APPROVALS",
                "CAP-O2C-COLLECTIONS", "CAP-O2C-RMA", "CAP-O2C-CREDITMEMO",
                "CAP-MFG-WOVARIANCE", "CAP-MFG-STOPPAGE",
                "CAP-PLAN-SAFETYSTOCK", "CAP-PLAN-CAPACITY",
                "CAP-INV-RESERVE",
                "CAP-QC-INSPECTION", "CAP-QC-NCR", "CAP-QC-CAPA",
                "CAP-MAINT-PM", "CAP-MAINT-BREAKDOWN",
                "CAP-IDEN-AUTH-KIOSK", "CAP-EXT-SHOPFLOOR-KIOSK", "CAP-EXT-ANDON",
                "CAP-HR-LEAVE", "CAP-HR-SHIFTS", "CAP-HR-TRAINING",
                "CAP-RPT-OEE",
            ]));

    public static PresetDefinition Preset05_RegulatedManufacturer { get; } = new(
        Id: "PRESET-05",
        Name: "Regulated Manufacturer",
        ShortDescription: "Manufacturer subject to industry compliance (ISO 13485, AS9100, IATF 16949, FDA, FSMA). Full QC stack — lots, ECO, gage, recall, training.",
        TargetProfile: "10–500 people (regulation orthogonal to size); industry cert; lot or serial traceability mandated; formal CAPA system.",
        EnabledCapabilities: AssemblePreset(
            remove: ["CAP-ACCT-BUILTIN"],
            add:
            [
                "CAP-ACCT-EXTERNAL",
                "CAP-MD-PRICELIST", "CAP-MD-ECO",
                "CAP-P2P-RFQ", "CAP-P2P-SUBCONTRACT", "CAP-P2P-APPROVALS",
                "CAP-O2C-COLLECTIONS", "CAP-O2C-RMA", "CAP-O2C-CREDITMEMO",
                "CAP-MFG-WOVARIANCE", "CAP-MFG-STOPPAGE",
                "CAP-PLAN-SAFETYSTOCK", "CAP-PLAN-CAPACITY",
                "CAP-INV-RESERVE", "CAP-INV-LOTS",
                "CAP-QC-INSPECTION", "CAP-QC-NCR", "CAP-QC-CAPA",
                "CAP-QC-GAGE", "CAP-QC-RECALL", "CAP-QC-COA",
                "CAP-QC-COMPLIANCE-FORMS",
                "CAP-MAINT-PM", "CAP-MAINT-BREAKDOWN",
                "CAP-IDEN-AUTH-KIOSK", "CAP-EXT-SHOPFLOOR-KIOSK", "CAP-EXT-ANDON",
                "CAP-EXT-ANNOUNCEMENTS",
                "CAP-HR-LEAVE", "CAP-HR-SHIFTS", "CAP-HR-TRAINING",
                "CAP-RPT-OEE",
            ]));

    public static PresetDefinition Preset06_MultiSite { get; } = new(
        Id: "PRESET-06",
        Name: "Multi-Site Operation",
        ShortDescription: "Manufacturer or distributor operating from 2+ plants with inter-site transfers, MRP/MPS, EDI, and corporate-level reporting.",
        TargetProfile: "50–500 people across sites; 2+ plants/warehouses; weekly+ inter-site transfers; corporate consolidation.",
        EnabledCapabilities: AssemblePreset(
            remove: ["CAP-ACCT-BUILTIN"],
            add:
            [
                "CAP-ACCT-EXTERNAL",
                "CAP-MD-PRICELIST", "CAP-MD-ECO", "CAP-MD-CONTRACTS-CONSIGNMENT",
                "CAP-P2P-RFQ", "CAP-P2P-SUBCONTRACT", "CAP-P2P-APPROVALS",
                "CAP-O2C-LEAD", "CAP-O2C-COLLECTIONS", "CAP-O2C-RMA", "CAP-O2C-CREDITMEMO",
                "CAP-MFG-WOVARIANCE", "CAP-MFG-STOPPAGE",
                "CAP-PLAN-SAFETYSTOCK", "CAP-PLAN-CAPACITY",
                "CAP-PLAN-MRP", "CAP-PLAN-MPS",
                "CAP-INV-RESERVE", "CAP-INV-MULTILOC",
                "CAP-QC-INSPECTION", "CAP-QC-NCR", "CAP-QC-CAPA",
                "CAP-MAINT-PM", "CAP-MAINT-BREAKDOWN",
                "CAP-IDEN-AUTH-KIOSK", "CAP-EXT-SHOPFLOOR-KIOSK", "CAP-EXT-ANDON",
                "CAP-EXT-ANNOUNCEMENTS",
                "CAP-CROSS-INTEG-EDI",
                "CAP-HR-LEAVE", "CAP-HR-SHIFTS", "CAP-HR-TRAINING",
                "CAP-RPT-OEE", "CAP-RPT-MRPEX",
            ]));

    public static PresetDefinition Preset07_Enterprise { get; } = new(
        Id: "PRESET-07",
        Name: "Enterprise",
        ShortDescription: "Large complex manufacturer / hybrid distributor with multi-currency, EDI, CPQ, APS, machine connect, BI export, SSO/MFA.",
        TargetProfile: "200+ people, multi-site, multi-currency, configure-to-order or engineer-to-order, formal supply-chain integration.",
        EnabledCapabilities: AssemblePreset(
            remove: ["CAP-ACCT-BUILTIN"],
            add:
            [
                "CAP-ACCT-EXTERNAL",
                "CAP-MD-PRICELIST", "CAP-MD-ECO", "CAP-MD-CONTRACTS-CONSIGNMENT",
                "CAP-P2P-RFQ", "CAP-P2P-AUTOPO", "CAP-P2P-DROPSHIP", "CAP-P2P-BACKTOBACK",
                "CAP-P2P-SUBCONTRACT", "CAP-P2P-APPROVALS",
                "CAP-O2C-LEAD", "CAP-O2C-CPQ", "CAP-O2C-RECURRING",
                "CAP-O2C-COLLECTIONS", "CAP-O2C-RMA", "CAP-O2C-CREDITMEMO",
                "CAP-MFG-BACKFLUSH", "CAP-MFG-WOVARIANCE", "CAP-MFG-STOPPAGE",
                "CAP-MFG-MACHINE-CONNECT",
                "CAP-PLAN-SAFETYSTOCK", "CAP-PLAN-CAPACITY",
                "CAP-PLAN-MRP", "CAP-PLAN-MPS", "CAP-PLAN-FORECAST",
                "CAP-PLAN-ATP", "CAP-PLAN-ABC",
                "CAP-INV-RESERVE", "CAP-INV-MULTILOC", "CAP-INV-PICKWAVE",
                "CAP-INV-PHYSICAL", "CAP-INV-LOTS",
                "CAP-QC-INSPECTION", "CAP-QC-NCR", "CAP-QC-CAPA",
                "CAP-QC-GAGE", "CAP-QC-RECALL",
                "CAP-MAINT-PM", "CAP-MAINT-BREAKDOWN", "CAP-MAINT-PREDICTIVE",
                "CAP-IDEN-AUTH-MFA", "CAP-IDEN-AUTH-SSO", "CAP-IDEN-AUTH-API-KEYS",
                "CAP-IDEN-AUTH-KIOSK",
                "CAP-EXT-SHOPFLOOR-KIOSK", "CAP-EXT-ANDON", "CAP-EXT-ANNOUNCEMENTS",
                "CAP-EXT-CHAT", "CAP-EXT-PROJECTS",
                "CAP-CROSS-INTEG-EDI", "CAP-CROSS-WEBHOOKS", "CAP-CROSS-BI-EXPORT",
                "CAP-HR-LEAVE", "CAP-HR-SHIFTS", "CAP-HR-PAYROLL",
                "CAP-HR-TRAINING", "CAP-HR-REVIEW",
                "CAP-RPT-OEE", "CAP-RPT-MRPEX",
            ]));

    public static PresetDefinition PresetCustom { get; } = new(
        Id: "PRESET-CUSTOM",
        Name: "Custom",
        ShortDescription: "Empty starting point — build the capability set yourself. Reachable from any point in discovery via the exit ramp.",
        TargetProfile: "Any business shape that doesn't fit one of the named presets, or any operator who wants explicit control.",
        // Per 4B Open Question 5 / Phase F decision: Custom inherits the 41
        // catalog defaults at apply-time (handled by the apply handler; the
        // catalog list is empty here so consumers can detect "Custom = no
        // preset-driven set").
        EnabledCapabilities: [],
        IsCustom: true);

    public static IReadOnlyList<PresetDefinition> All { get; } = new List<PresetDefinition>
    {
        Preset01_TwoPersonShop,
        Preset02_GrowingJobShop,
        Preset03_Distribution,
        Preset04_ProductionManufacturer,
        Preset05_RegulatedManufacturer,
        Preset06_MultiSite,
        Preset07_Enterprise,
        PresetCustom,
    };

    public static PresetDefinition? FindById(string id) =>
        All.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.Ordinal));
}
