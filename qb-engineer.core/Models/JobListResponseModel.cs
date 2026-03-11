namespace QBEngineer.Core.Models;

public record JobListResponseModel(
    int Id,
    string JobNumber,
    string Title,
    string StageName,
    string StageColor,
    string? AssigneeInitials,
    string? AssigneeColor,
    string PriorityName,
    DateTime? DueDate,
    bool IsOverdue,
    string? CustomerName,
    string? BillingStatus,
    string? Disposition,
    int ChildJobCount,
    string? ExternalRef,
    string? AccountingDocumentType);
