namespace QBEngineer.Core.Entities;

public class Shift : BaseAuditableEntity
{
    public string Name { get; set; } = "";
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int BreakMinutes { get; set; }
    public decimal NetHours { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<WorkCenterShift> WorkCenterShifts { get; set; } = [];
}
