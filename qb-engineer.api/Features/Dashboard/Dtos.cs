namespace QBEngineer.Api.Features.Dashboard;

public record DashboardDto(
    List<DashboardTaskDto> Tasks,
    List<StageCountDto> Stages,
    List<TeamMemberDto> Team,
    List<ActivityEntryDto> Activity,
    List<DeadlineEntryDto> Deadlines,
    DashboardKPIsDto KPIs);

public record DashboardTaskDto(
    string Time,
    string Title,
    string JobNumber,
    string BarColor,
    AssigneeInfo Assignee,
    string Status,
    string StatusColor);

public record AssigneeInfo(string Initials, string Color);

public record StageCountDto(string Label, int Count, string Color, int MaxCount);

public record TeamMemberDto(string Initials, string Name, string Color, int TaskCount, int MaxTasks);

public record ActivityEntryDto(string Icon, string IconColor, string Text, string Time);

public record DeadlineEntryDto(string Date, string JobNumber, string Description, bool IsOverdue);

public record DashboardKPIsDto(
    int ActiveCount,
    int ActiveChange,
    int OverdueCount,
    int OverdueChange,
    string TotalHours,
    string HoursStatus);
