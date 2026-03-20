using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record ComplianceFormTemplateResponseModel(
    int Id,
    string Name,
    ComplianceFormType FormType,
    string Description,
    string Icon,
    string? SourceUrl,
    bool IsAutoSync,
    bool IsActive,
    int SortOrder,
    bool RequiresIdentityDocs,
    int? DocuSealTemplateId,
    DateTime? LastSyncedAt,
    int? ManualOverrideFileId,
    bool BlocksJobAssignment,
    string ProfileCompletionKey,
    int? CurrentFormDefinitionVersionId,
    string? FormDefinitionJson,
    string? FormDefinitionRevision,
    DateTime CreatedAt,
    DateTime UpdatedAt);
