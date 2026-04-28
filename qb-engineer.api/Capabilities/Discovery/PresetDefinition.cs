namespace QBEngineer.Api.Capabilities.Discovery;

/// <summary>
/// Phase 4 Phase-F — Preset definition. Each preset names a known business
/// profile (Two-Person Shop, Growing Job Shop, etc.) and spells out which
/// capabilities should be enabled when the preset is applied.
///
/// Mirrors the Phase 4B design (preset-design.md). The preset catalog is
/// static; mutation lives at the capability level (preset apply is just a
/// bulk-toggle to the preset's target state).
///
/// <see cref="EnabledCapabilities"/> is the FULL set of capabilities the
/// preset wants enabled — both default-on entries (always on regardless)
/// and explicit additions. Capabilities NOT in this set are disabled when
/// the preset is applied. PRESET-CUSTOM has an empty list (no defaults
/// added; per 4B Open Question 5 / 4F-decisions-log, Custom inherits the
/// 41 catalog defaults from <see cref="CapabilityCatalog"/> at apply time).
/// </summary>
public record PresetDefinition(
    string Id,
    string Name,
    string ShortDescription,
    string TargetProfile,
    IReadOnlyList<string> EnabledCapabilities,
    bool IsCustom = false);
