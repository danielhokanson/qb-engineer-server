using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Onboarding;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/onboarding")]
[Authorize]
public class OnboardingController(IMediator mediator) : ControllerBase
{
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetEmail() => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    private string GetName() => User.FindFirstValue(ClaimTypes.Name) ?? GetEmail();

    /// <summary>
    /// Submits the unified onboarding wizard. Fills government PDFs, submits to DocuSeal,
    /// persists profile data, and marks acknowledgments complete.
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<OnboardingSubmitResultModel>> Submit(
        [FromBody] OnboardingSubmitRequestModel model, CancellationToken ct)
    {
        var result = await mediator.Send(
            new SubmitOnboardingCommand(GetUserId(), GetEmail(), GetName(), model), ct);
        return Ok(result);
    }

    /// <summary>
    /// Pre-uploads an I-9 identity document (List A, B, or C) before the wizard is submitted.
    /// Returns a FileAttachmentId to be included in the main submission payload.
    /// </summary>
    [HttpPost("i9-document")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadI9DocumentResultModel>> UploadI9Document(
        IFormFile file,
        [FromForm] string documentList,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UploadI9DocumentCommand(GetUserId(), file, documentList), ct);
        return Ok(result);
    }

    /// <summary>Returns completion status for all onboarding items for the current user.</summary>
    [HttpGet("status")]
    public async Task<ActionResult<OnboardingStatusModel>> GetStatus(CancellationToken ct)
    {
        var result = await mediator.Send(new GetOnboardingStatusQuery(GetUserId()), ct);
        return Ok(result);
    }
}
