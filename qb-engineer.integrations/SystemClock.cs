using QBEngineer.Core.Interfaces;

namespace QBEngineer.Integrations;

/// <summary>Real-time clock — delegates to DateTimeOffset.UtcNow.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
