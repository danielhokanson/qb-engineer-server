using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.TimeTracking;

public record CreateClockEventCommand(CreateClockEventRequestModel Data) : IRequest<ClockEventResponseModel>;

public class CreateClockEventValidator : AbstractValidator<CreateClockEventCommand>
{
    public CreateClockEventValidator()
    {
        RuleFor(x => x.Data.EventType).IsInEnum();
        RuleFor(x => x.Data.Reason).MaximumLength(500).When(x => x.Data.Reason is not null);
        RuleFor(x => x.Data.ScanMethod).MaximumLength(50).When(x => x.Data.ScanMethod is not null);
        RuleFor(x => x.Data.Source).MaximumLength(50).When(x => x.Data.Source is not null);
    }
}

public class CreateClockEventHandler(ITimeTrackingRepository repo, IHttpContextAccessor httpContext) : IRequestHandler<CreateClockEventCommand, ClockEventResponseModel>
{
    public async Task<ClockEventResponseModel> Handle(CreateClockEventCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var data = request.Data;

        var clockEvent = new ClockEvent
        {
            UserId = userId,
            EventType = data.EventType,
            Reason = data.Reason?.Trim(),
            ScanMethod = data.ScanMethod?.Trim(),
            Timestamp = DateTimeOffset.UtcNow,
            Source = data.Source?.Trim(),
        };

        await repo.AddClockEventAsync(clockEvent, cancellationToken);

        // Return populated response
        var user = httpContext.HttpContext!.User;
        var userName = user.FindFirstValue(ClaimTypes.GivenName) + " " + user.FindFirstValue(ClaimTypes.Surname);

        return new ClockEventResponseModel(
            clockEvent.Id, userId, userName.Trim(), clockEvent.EventType,
            clockEvent.Reason, clockEvent.ScanMethod, clockEvent.Timestamp, clockEvent.Source);
    }
}
