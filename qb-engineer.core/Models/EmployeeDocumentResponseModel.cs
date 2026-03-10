namespace QBEngineer.Core.Models;

public record EmployeeDocumentResponseModel(
    int Id,
    string FileName,
    string ContentType,
    long Size,
    string? DocumentType,
    DateTime? ExpirationDate,
    DateTime CreatedAt);
