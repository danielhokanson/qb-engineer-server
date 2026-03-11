using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record DisposeJobRequestModel(
    JobDisposition Disposition,
    string? Notes);
