namespace QBEngineer.Core.Models;

public record TimeByUserReportItem(
    int UserId,
    string UserName,
    decimal TotalHours);
