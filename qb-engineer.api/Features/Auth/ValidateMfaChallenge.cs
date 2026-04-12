using FluentValidation;
using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Auth;

public record ValidateMfaChallengeCommand(string ChallengeToken, string Code, bool RememberDevice) : IRequest<MfaValidateResponseModel?>;

public class ValidateMfaChallengeValidator : AbstractValidator<ValidateMfaChallengeCommand>
{
    public ValidateMfaChallengeValidator()
    {
        RuleFor(x => x.ChallengeToken).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches(@"^\d{6}$");
    }
}

public class ValidateMfaChallengeHandler(IMfaService mfaService) : IRequestHandler<ValidateMfaChallengeCommand, MfaValidateResponseModel?>
{
    public async Task<MfaValidateResponseModel?> Handle(ValidateMfaChallengeCommand request, CancellationToken cancellationToken)
    {
        return await mfaService.ValidateChallengeAsync(request.ChallengeToken, request.Code, request.RememberDevice, cancellationToken);
    }
}
