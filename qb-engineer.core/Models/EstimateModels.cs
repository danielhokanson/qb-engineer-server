using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record EstimateListItemModel(
    int Id,
    int CustomerId,
    string CustomerName,
    string Title,
    decimal EstimatedAmount,
    EstimateStatus Status,
    DateTimeOffset? ValidUntil,
    int? ConvertedToQuoteId,
    string? AssignedToName,
    DateTimeOffset CreatedAt);

public record EstimateDetailResponseModel(
    int Id,
    int CustomerId,
    string CustomerName,
    string Title,
    string? Description,
    decimal EstimatedAmount,
    EstimateStatus Status,
    DateTimeOffset? ValidUntil,
    string? Notes,
    int? AssignedToId,
    string? AssignedToName,
    int? ConvertedToQuoteId,
    DateTimeOffset? ConvertedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateEstimateRequestModel(
    int CustomerId,
    string Title,
    string? Description,
    decimal EstimatedAmount,
    DateTimeOffset? ValidUntil,
    string? Notes,
    int? AssignedToId);

public record UpdateEstimateRequestModel(
    string? Title,
    string? Description,
    decimal? EstimatedAmount,
    EstimateStatus? Status,
    DateTimeOffset? ValidUntil,
    string? Notes,
    int? AssignedToId);
