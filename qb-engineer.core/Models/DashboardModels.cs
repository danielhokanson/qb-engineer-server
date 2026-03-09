namespace QBEngineer.Core.Models;

public record DashboardResponseModel(
    List<DashboardTaskResponseModel> Tasks,
    List<StageCountResponseModel> Stages,
    List<TeamMemberResponseModel> Team,
    List<ActivityEntryResponseModel> Activity,
    List<DeadlineEntryResponseModel> Deadlines,
    DashboardKPIsResponseModel KPIs);

public record DashboardTaskResponseModel(
    string Time,
    string Title,
    string JobNumber,
    string BarColor,
    AssigneeInfo Assignee,
    string Status,
    string StatusColor);

public record AssigneeInfo(string Initials, string Color);

public record StageCountResponseModel(string Label, int Count, string Color, int MaxCount);

public record TeamMemberResponseModel(string Initials, string Name, string Color, int TaskCount, int MaxTasks);

public record ActivityEntryResponseModel(string Icon, string IconColor, string Text, string Time);

public record DeadlineEntryResponseModel(string Date, string JobNumber, string Description, bool IsOverdue);

public record DashboardKPIsResponseModel(
    int ActiveCount,
    int ActiveChange,
    int OverdueCount,
    int OverdueChange,
    string TotalHours,
    string HoursStatus);
