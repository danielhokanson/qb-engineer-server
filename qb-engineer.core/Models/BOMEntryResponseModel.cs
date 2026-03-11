using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record BOMEntryResponseModel(
    int Id,
    int ChildPartId,
    string ChildPartNumber,
    string ChildDescription,
    decimal Quantity,
    string? ReferenceDesignator,
    int SortOrder,
    BOMSourceType SourceType,
    int? LeadTimeDays,
    string? Notes);
