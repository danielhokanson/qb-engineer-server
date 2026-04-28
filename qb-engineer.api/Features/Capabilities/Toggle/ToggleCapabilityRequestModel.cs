namespace QBEngineer.Api.Features.Capabilities.Toggle;

/// <summary>
/// Phase 4 Phase-B — Body for <c>PUT /api/v1/capabilities/{id}/enabled</c>.
/// </summary>
public record ToggleCapabilityRequestModel(bool Enabled);
