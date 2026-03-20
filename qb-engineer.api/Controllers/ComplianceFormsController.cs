using System.Security.Claims;
using System.Text.Json;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.ComplianceForms;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/compliance-forms")]
[Authorize]
public class ComplianceFormsController(IMediator mediator) : ControllerBase
{
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ComplianceFormTemplateResponseModel>>> GetAll(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAllComplianceTemplatesQuery(includeInactive), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ComplianceFormTemplateResponseModel>> Get(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetComplianceTemplateQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ComplianceFormTemplateResponseModel>> Create(
        [FromBody] CreateComplianceFormTemplateRequestModel model, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateComplianceTemplateCommand(model), ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id,
        [FromBody] UpdateComplianceFormTemplateRequestModel model, CancellationToken ct)
    {
        await mediator.Send(new UpdateComplianceTemplateCommand(id, model), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/upload")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ComplianceFormTemplateResponseModel>> UploadDocument(
        int id, [FromBody] UploadTemplateDocumentRequestModel model, CancellationToken ct)
    {
        var result = await mediator.Send(new UploadTemplateDocumentCommand(id, model.FileAttachmentId), ct);
        return Ok(result);
    }

    [HttpPut("{id:int}/form-definition")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ComplianceFormTemplateResponseModel>> UpdateFormDefinition(
        int id, [FromBody] UpdateFormDefinitionRequestModel model, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateFormDefinitionCommand(id, model.FormDefinitionJson, model.Revision), ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/extract-definition")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ExtractFormDefinitionResult>> ExtractDefinition(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new ExtractFormDefinitionCommand(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Diagnostic: dump raw pdf.js extraction data for a compliance template's PDF.
    /// </summary>
    [HttpPost("{id:int}/extract-raw")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExtractRaw(
        int id,
        [FromServices] QBEngineer.Core.Interfaces.IPdfJsExtractorService pdfJsExtractor,
        [FromServices] QBEngineer.Core.Interfaces.IStorageService storageService,
        [FromServices] IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        byte[]? pdfBytes = null;

        var db = HttpContext.RequestServices.GetRequiredService<QBEngineer.Data.Context.AppDbContext>();
        var dbTemplate = await db.ComplianceFormTemplates.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Template {id} not found");

        if (dbTemplate.ManualOverrideFileId.HasValue)
        {
            var file = await db.FileAttachments.FindAsync([dbTemplate.ManualOverrideFileId.Value], ct);
            if (file is not null)
            {
                using var stream = await storageService.DownloadAsync(file.BucketName, file.ObjectKey, ct);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, ct);
                pdfBytes = ms.ToArray();
            }
        }

        if (pdfBytes is null && !string.IsNullOrEmpty(dbTemplate.SourceUrl))
        {
            using var httpClient = httpClientFactory.CreateClient();
            pdfBytes = await httpClient.GetByteArrayAsync(dbTemplate.SourceUrl, ct);
        }

        if (pdfBytes is null)
            return BadRequest("No PDF source available");

        var rawResult = await pdfJsExtractor.ExtractRawAsync(pdfBytes, ct);
        return Ok(rawResult);
    }

    [HttpPost("{id:int}/compare-visual")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VisualComparisonResult>> CompareVisual(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new CompareFormRenderingCommand(id), ct);
        return Ok(result);
    }

    [HttpGet("versions/{versionId:int}/comparison")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VisualComparisonResult>> GetComparisonResult(int versionId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetComparisonResultQuery(versionId), ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteComplianceTemplateCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/sync")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Sync(int id, CancellationToken ct)
    {
        await mediator.Send(new SyncComplianceTemplateCommand(id), ct);
        return NoContent();
    }

    [HttpPost("sync-all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> SyncAll(CancellationToken ct)
    {
        var count = await mediator.Send(new SyncAllComplianceTemplatesCommand(), ct);
        return Ok(count);
    }

    [HttpGet("my-state-definition")]
    public async Task<ActionResult<StateFormDefinitionResult>> GetMyStateFormDefinition(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyStateFormDefinitionQuery(GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("submissions/me")]
    public async Task<ActionResult<List<ComplianceFormSubmissionResponseModel>>> GetMySubmissions(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMySubmissionsQuery(GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("submissions/me/{formType}")]
    public async Task<ActionResult<ComplianceFormSubmissionResponseModel?>> GetMySubmissionByType(
        ComplianceFormType formType, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMySubmissionByTypeQuery(GetUserId(), formType), ct);
        return Ok(result);
    }

    [HttpPut("{id:int}/form-data")]
    public async Task<ActionResult<ComplianceFormSubmissionResponseModel>> SaveFormData(
        int id, [FromBody] SaveFormDataRequestModel model, CancellationToken ct)
    {
        var result = await mediator.Send(new SaveFormDataCommand(GetUserId(), id, model.FormDataJson, model.FormDefinitionVersionId), ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/submit-form")]
    public async Task<ActionResult<ComplianceFormSubmissionResponseModel>> SubmitFormData(
        int id, [FromBody] SaveFormDataRequestModel model, CancellationToken ct)
    {
        var result = await mediator.Send(new SubmitFormDataCommand(GetUserId(), id, model.FormDataJson, model.FormDefinitionVersionId), ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<ComplianceFormSubmissionResponseModel>> CreateSubmission(
        int id, CancellationToken ct)
    {
        var userId = GetUserId();
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var name = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var result = await mediator.Send(new CreateSubmissionCommand(userId, id, email, name), ct);
        return CreatedAtAction(nameof(GetMySubmissions), result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);
        var payload = JsonDocument.Parse(body);

        var eventType = payload.RootElement.GetProperty("event_type").GetString();
        if (eventType != "form.completed")
            return Ok();

        var data = payload.RootElement.GetProperty("data");
        var submissionId = data.GetProperty("id").GetInt32();
        var status = data.GetProperty("status").GetString() ?? "completed";
        DateTime? completedAt = data.TryGetProperty("completed_at", out var completedProp)
            ? completedProp.GetDateTime()
            : DateTime.UtcNow;

        await mediator.Send(new HandleDocuSealWebhookCommand(submissionId, status, completedAt), ct);
        return Ok();
    }

    [HttpGet("admin/users/{userId:int}")]
    [Authorize(Roles = "Admin,Manager,OfficeManager")]
    public async Task<ActionResult<UserComplianceDetailResponseModel>> GetUserComplianceDetail(
        int userId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserComplianceDetailQuery(userId), ct);
        return Ok(result);
    }

    [HttpPost("admin/users/{userId:int}/remind")]
    [Authorize(Roles = "Admin,Manager,OfficeManager")]
    public async Task<IActionResult> SendReminder(int userId, CancellationToken ct)
    {
        await mediator.Send(new SendComplianceReminderCommand(userId, GetUserId()), ct);
        return NoContent();
    }
}
