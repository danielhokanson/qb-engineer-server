using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;
using QBEngineer.Tests.Capabilities;

namespace QBEngineer.Tests.Workflows;

[Collection(CapabilityTestCollection.Name)]
public class WorkflowRunLifecycleTests(CapabilityTestWebApplicationFactory factory)
{
    private HttpClient AuthenticatedClient(string role = "Admin")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "1");
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }

    [Fact]
    public async Task Start_CreatesDraftPart_AndRun()
    {
        var client = AuthenticatedClient();
        var initial = JsonDocument.Parse("""{"description":"Lifecycle Test Widget","partType":"Part","material":"Steel"}""").RootElement;
        var body = new StartWorkflowRunRequestModel(
            "Part", "part-assembly-guided-v1", "guided", initial);

        var response = await client.PostAsJsonAsync("/api/v1/workflows", body);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var run = await response.Content.ReadFromJsonAsync<WorkflowRunResponseModel>();
        run!.EntityType.Should().Be("Part");
        run!.EntityId.Should().BeGreaterThan(0);
        run!.CurrentStepId.Should().Be("basics");
        run!.Mode.Should().Be("guided");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var part = await db.Parts.FirstAsync(p => p.Id == run.EntityId);
        part.Status.Should().Be(PartStatus.Draft);

        // Audit row recorded.
        var audit = await db.AuditLogEntries
            .Where(a => a.EntityType == "WorkflowRun" && a.EntityId == run.Id && a.Action == "WorkflowStarted")
            .ToListAsync();
        audit.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Start_RejectsMismatchedDefinitionEntityType()
    {
        var client = AuthenticatedClient();
        var body = new StartWorkflowRunRequestModel(
            "Customer", "part-assembly-guided-v1", null, null);
        var response = await client.PostAsJsonAsync("/api/v1/workflows", body);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PatchStep_AdvancesCurrentStep_WhenGatePasses()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        // basics step has all three required fields. Send them and expect advance.
        var fields = JsonDocument.Parse("""{"description":"Adv Test","material":"Steel","partType":"Part"}""").RootElement;
        var step = new PatchWorkflowStepRequestModel("basics", fields);
        var response = await client.PatchAsJsonAsync($"/api/v1/workflows/{run.Id}/step", step);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<WorkflowRunResponseModel>();
        updated!.CurrentStepId.Should().Be("bom");
    }

    [Fact]
    public async Task PatchStep_DoesNotAdvance_WhenGateFails()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        // Provide only description; partType and material remain unset → hasBasics fails.
        // We have to clear the seed values that StartRun applies — start with empty initial.
        var fields = JsonDocument.Parse("""{"description":"Only desc"}""").RootElement;
        var step = new PatchWorkflowStepRequestModel("basics", fields);
        var response = await client.PatchAsJsonAsync($"/api/v1/workflows/{run.Id}/step", step);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<WorkflowRunResponseModel>();
        // Without setting material, the createDraft default doesn't supply it;
        // the part defaulted to PartType=Part. material is null → hasBasics fails.
        // The pointer should remain on basics.
        updated!.CurrentStepId.Should().Be("basics");
    }

    [Fact]
    public async Task Jump_BackToCompletedStep_Allowed()
    {
        var client = AuthenticatedClient();
        var initial = JsonDocument.Parse("""{"description":"Jumper","partType":"Part","material":"Steel"}""").RootElement;
        var run = await StartRunAsync(client, initial.GetRawText());

        // Advance past basics with a gate-passing patch.
        await client.PatchAsJsonAsync($"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel("basics", initial));

        // Jump back to basics.
        var jumpResp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/jump",
            new JumpWorkflowRequestModel("basics"));
        jumpResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Jump_ForwardWithFailingGate_409()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, """{"description":"Skipper"}"""); // missing material → hasBasics fails

        var jumpResp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/jump",
            new JumpWorkflowRequestModel("bom"));
        jumpResp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Mode_Toggle_PersistsChange()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        var resp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/mode",
            new SetWorkflowModeRequestModel("express"));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await resp.Content.ReadFromJsonAsync<WorkflowRunResponseModel>();
        updated!.Mode.Should().Be("express");
    }

    [Fact]
    public async Task Abandon_SoftDeletesDraftEntity()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        var resp = await client.PostAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/abandon",
            new AbandonWorkflowRequestModel("user"));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var part = await db.Parts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == run.EntityId);
        part!.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Complete_FailsWhenReadinessUnsatisfied()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, """{"description":"Incomplete"}""");
        var resp = await client.PostAsync($"/api/v1/workflows/{run.Id}/complete", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("missing").GetArrayLength().Should().BeGreaterThan(0);
        doc.RootElement.GetProperty("code").GetString().Should().Be("workflow-readiness-missing");
    }

    [Fact]
    public async Task PromoteStatus_FailsWhenReadinessUnsatisfied()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, """{"description":"NotReady"}""");
        var resp = await client.PostAsJsonAsync(
            $"/api/v1/parts/{run.EntityId}/promote-status",
            new PromoteEntityStatusRequestModel("Active"));
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PromoteStatus_SucceedsWhenAllValidatorsPass()
    {
        var client = AuthenticatedClient();
        var initial = JsonDocument.Parse(
            """{"description":"Ready","partType":"Part","material":"Steel","manualCostOverride":12.5}""").RootElement;
        var run = await StartRunAsync(client, initial.GetRawText());

        // Add BOM + Operation rows directly (these are usually edited via
        // their dedicated endpoints in the workflow's BOM/Routing steps).
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var component = new Part
            {
                PartNumber = $"TC-{Guid.NewGuid():N}"[..16],
                Description = "Component",
                PartType = PartType.Part,
                Status = PartStatus.Active,
            };
            db.Parts.Add(component);
            await db.SaveChangesAsync();
            db.BOMEntries.Add(new BOMEntry
            {
                ParentPartId = run.EntityId,
                ChildPartId = component.Id,
                Quantity = 1,
            });
            db.Operations.Add(new Operation
            {
                PartId = run.EntityId,
                StepNumber = 10,
                Title = "OP10",
            });
            await db.SaveChangesAsync();
        }

        var resp = await client.PostAsJsonAsync(
            $"/api/v1/parts/{run.EntityId}/promote-status",
            new PromoteEntityStatusRequestModel("Active"));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        // PartDetailResponseModel.Status is a PartStatus enum, but the server
        // emits it as a string via JsonStringEnumConverter — verify against
        // the raw JSON to avoid wiring up converters in the test client.
        var rawJson = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(rawJson);
        doc.RootElement.GetProperty("status").GetString().Should().Be("Active");

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var refreshed = await ctx.WorkflowRuns.FirstAsync(r => r.Id == run.Id);
        refreshed.CompletedAt.Should().NotBeNull("promote should also complete the in-flight run");
    }

    [Fact]
    public async Task ListActive_ReturnsOnlyCurrentUserInFlightRuns()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        var rows = await client.GetFromJsonAsync<List<WorkflowRunResponseModel>>("/api/v1/workflows/active");
        rows!.Should().Contain(r => r.Id == run.Id);

        // After abandon it should drop out.
        await client.PostAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/abandon",
            new AbandonWorkflowRequestModel("user"));
        var rowsAfter = await client.GetFromJsonAsync<List<WorkflowRunResponseModel>>("/api/v1/workflows/active");
        rowsAfter!.Should().NotContain(r => r.Id == run.Id);
    }

    [Fact]
    public async Task StepAdvance_EmitsAuditRow()
    {
        var client = AuthenticatedClient();
        var initial = JsonDocument.Parse("""{"description":"Auditable","partType":"Part","material":"Steel"}""").RootElement;
        var run = await StartRunAsync(client, initial.GetRawText());

        await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel("basics", initial));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var audit = await db.AuditLogEntries
            .Where(a => a.EntityType == "WorkflowRun" && a.EntityId == run.Id && a.Action == "WorkflowStepAdvanced")
            .ToListAsync();
        audit.Should().NotBeEmpty();
    }

    private async Task<WorkflowRunResponseModel> StartRunAsync(HttpClient client, string initialJson)
    {
        var initial = JsonDocument.Parse(initialJson).RootElement;
        var body = new StartWorkflowRunRequestModel(
            "Part", "part-assembly-guided-v1", "guided", initial);
        var response = await client.PostAsJsonAsync("/api/v1/workflows", body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorkflowRunResponseModel>())!;
    }
}
