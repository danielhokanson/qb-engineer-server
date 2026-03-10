namespace QBEngineer.Core.Models;

public record CustomerActivityReportItem(
    int CustomerId,
    string CustomerName,
    int ActiveJobs,
    int CompletedJobs,
    int TotalJobs,
    DateTime? LastJobDate);
