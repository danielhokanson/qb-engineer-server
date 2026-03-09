namespace QBEngineer.Core.Models;

public record DashboardTaskResponseModel(
    string Time,
    string Title,
    string JobNumber,
    string BarColor,
    AssigneeInfo Assignee,
    string Status,
    string StatusColor);
