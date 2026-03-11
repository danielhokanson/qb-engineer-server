namespace QBEngineer.Core.Models;

public record SetStatusRequestModel(
    string StatusCode,
    string? Notes);
