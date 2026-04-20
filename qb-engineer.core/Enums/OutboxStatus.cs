namespace QBEngineer.Core.Enums;

public enum OutboxStatus
{
    Pending,
    InFlight,
    Sent,
    Failed,
    DeadLetter,
}
