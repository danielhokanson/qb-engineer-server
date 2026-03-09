namespace QBEngineer.Core.Models;

public record DashboardResponseModel(
    List<DashboardTaskResponseModel> Tasks,
    List<StageCountResponseModel> Stages,
    List<TeamMemberResponseModel> Team,
    List<ActivityEntryResponseModel> Activity,
    List<DeadlineEntryResponseModel> Deadlines,
    DashboardKPIsResponseModel KPIs);
