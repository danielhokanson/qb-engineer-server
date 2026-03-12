using FluentValidation;
using MediatR;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ShopFloor;

public record ClockInOutCommand(int UserId, string EventType) : IRequest;

public class ClockInOutValidator : AbstractValidator<ClockInOutCommand>
{
    public ClockInOutValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.EventType)
            .Must(t => t is "ClockIn" or "ClockOut" or "BreakStart" or "BreakEnd")
            .WithMessage("EventType must be ClockIn, ClockOut, BreakStart, or BreakEnd");
    }
}

public class ClockInOutHandler(AppDbContext db)
    : IRequestHandler<ClockInOutCommand>
{
    public async Task Handle(ClockInOutCommand request, CancellationToken ct)
    {
        _ = await db.Users.FindAsync([request.UserId], ct)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found");

        var eventType = Enum.Parse<ClockEventType>(request.EventType);

        db.ClockEvents.Add(new ClockEvent
        {
            UserId = request.UserId,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            Source = "kiosk",
        });

        await db.SaveChangesAsync(ct);
    }
}
