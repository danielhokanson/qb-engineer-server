namespace QBEngineer.Api.Features.Presets.Models;

/// <summary>
/// Phase 4 Phase-G — Response models for the preset browser surface.
///
/// Mirrors the static <see cref="QBEngineer.Api.Capabilities.Discovery.PresetCatalog"/>
/// data plus per-install delta information (vs catalog defaults, vs current
/// install state) so the UI can render the browser/detail/compare/custom
/// surfaces without re-implementing the delta math.
/// </summary>
public record PresetSummaryResponseModel(
    string Id,
    string Name,
    string ShortDescription,
    string TargetProfile,
    int CapabilityCount,
    bool IsCustom,
    bool IsActive,
    IReadOnlyList<string> RecommendedFor);

public record PresetDetailResponseModel(
    string Id,
    string Name,
    string ShortDescription,
    string TargetProfile,
    int CapabilityCount,
    bool IsCustom,
    bool IsActive,
    IReadOnlyList<string> RecommendedFor,
    IReadOnlyList<PresetCapabilityRowResponseModel> Capabilities,
    IReadOnlyList<PresetCapabilityRowResponseModel> DeltaVsCatalogDefaults,
    IReadOnlyList<PresetCapabilityDeltaResponseModel> DeltaVsCurrentInstall);

public record PresetCapabilityRowResponseModel(
    string Code,
    string Name,
    string Area,
    string Description,
    bool InPreset,
    bool DefaultOn);

public record PresetCapabilityDeltaResponseModel(
    string Code,
    string Name,
    string Area,
    bool CurrentlyEnabled,
    bool WillBeEnabled);

public record PresetCompareCellResponseModel(
    string PresetId,
    bool InPreset);

public record PresetCompareCapabilityRowResponseModel(
    string Code,
    string Name,
    string Area,
    bool DefaultOn,
    IReadOnlyList<PresetCompareCellResponseModel> Cells,
    bool Disagreement);

public record PresetCompareResponseModel(
    IReadOnlyList<PresetSummaryResponseModel> Presets,
    IReadOnlyList<PresetCompareCapabilityRowResponseModel> Rows);

public record PresetApplyPreviewResponseModel(
    string PresetId,
    string PresetName,
    bool IsCustom,
    int DeltaCount,
    IReadOnlyList<PresetCapabilityDeltaResponseModel> Deltas,
    bool Valid,
    IReadOnlyList<PresetApplyViolationResponseModel> Violations);

public record PresetApplyViolationResponseModel(
    string Code,
    string Capability,
    string Message,
    IReadOnlyList<string>? Missing,
    IReadOnlyList<string>? Conflicts,
    IReadOnlyList<string>? Dependents);

public record PresetApplyResultResponseModel(
    string PresetId,
    string PresetName,
    bool IsCustom,
    bool NoOp,
    int DeltaCount,
    IReadOnlyList<PresetCapabilityDeltaResponseModel> Applied);

public record PresetCustomOverrideRequestItem(string Code, bool Enabled);

public record PresetCustomPreviewRequestModel(
    IReadOnlyList<PresetCustomOverrideRequestItem> CapabilityOverrides);

public record PresetCustomApplyRequestModel(
    IReadOnlyList<PresetCustomOverrideRequestItem> CapabilityOverrides,
    string? Reason);

public record PresetApplyRequestModel(string? Reason);

public record PresetCompareRequestModel(IReadOnlyList<string> PresetIds);

public record PresetCustomPreviewResponseModel(
    int CapabilityCount,
    IReadOnlyList<PresetCapabilityRowResponseModel> Capabilities,
    IReadOnlyList<PresetCapabilityDeltaResponseModel> DeltaVsCurrentInstall,
    bool Valid,
    IReadOnlyList<PresetApplyViolationResponseModel> Violations);
