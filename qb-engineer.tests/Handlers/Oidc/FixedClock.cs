using QBEngineer.Core.Interfaces;

namespace QBEngineer.Tests.Handlers.Oidc;

internal sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset now) => UtcNow = now;
    public DateTimeOffset UtcNow { get; set; }
}
