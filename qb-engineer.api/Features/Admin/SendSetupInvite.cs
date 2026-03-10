using MediatR;
using Microsoft.AspNetCore.Identity;
using QBEngineer.Api.Jobs;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record SendSetupInviteCommand(int UserId, string BaseUrl) : IRequest;

public class SendSetupInviteHandler(
    UserManager<ApplicationUser> userManager,
    IMediator mediator,
    IEmailService emailService,
    ISystemSettingRepository settings) : IRequestHandler<SendSetupInviteCommand>
{
    public async Task Handle(SendSetupInviteCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new KeyNotFoundException($"User {request.UserId} not found");

        // Generate setup token if none exists
        if (string.IsNullOrEmpty(user.SetupToken) || user.SetupTokenExpiresAt < DateTime.UtcNow)
        {
            var tokenResult = await mediator.Send(new GenerateSetupTokenCommand(request.UserId), cancellationToken);
            user.SetupToken = tokenResult.Token;
        }

        var companySetting = await settings.FindByKeyAsync("company_name", cancellationToken);
        var companyName = companySetting?.Value ?? "QB Engineer";

        var setupUrl = $"{request.BaseUrl.TrimEnd('/')}/setup/{user.SetupToken}";

        var html = EmailTemplateBuilder.BuildNotificationEmail(
            companyName,
            user.FirstName,
            "Welcome! Complete Your Account Setup",
            $"You've been invited to join {companyName}. Click the link below to set your password and complete your account setup.<br><br>" +
            $"<a href=\"{setupUrl}\" style=\"display:inline-block;padding:12px 24px;background:#1e293b;color:white;text-decoration:none;font-weight:600;\">Complete Setup</a><br><br>" +
            $"This link expires in 7 days. If the button doesn't work, copy and paste this URL:<br>{setupUrl}");

        await emailService.SendAsync(new EmailMessage(
            user.Email!,
            $"[{companyName}] Complete Your Account Setup",
            html));
    }
}
