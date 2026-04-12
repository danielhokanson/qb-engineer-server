using System.Security.Claims;

using FluentValidation;
using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Auth;

public record VerifyMfaSetupCommand(ClaimsPrincipal User, int DeviceId, string Code) : IRequest<bool>;

public class VerifyMfaSetupValidator : AbstractValidator<VerifyMfaSetupCommand>
{
    public VerifyMfaSetupValidator()
    {
        RuleFor(x => x.DeviceId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches(@"^\d{6}$");
    }
}

public class VerifyMfaSetupHandler(IMfaService mfaService) : IRequestHandler<VerifyMfaSetupCommand, bool>
{
    public async Task<bool> Handle(VerifyMfaSetupCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(request.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());

        return await mfaService.VerifyTotpSetupAsync(userId, request.DeviceId, request.Code, cancellationToken);
    }
}
