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
    public async Task Start_DefersEntityCreation_AndStashesPayload()
    {
        var client = AuthenticatedClient();
        var initial = JsonDocument.Parse("""{"name":"Lifecycle Test Widget","partType":"Part","material":"Steel"}""").RootElement;
        var body = new StartWorkflowRunRequestModel(
            "Part", "part-assembly-guided-v1", "guided", initial);

        var response = await client.PostAsJsonAsync("/api/v1/workflows", body);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var run = await response.Content.ReadFromJsonAsync<WorkflowRunResponseModel>();
        run!.EntityType.Should().Be("Part");
        run!.EntityId.Should().BeNull("entity row materializes on first step patch, not at workflow start");
        run!.CurrentStepId.Should().Be("basics");
        run!.Mode.Should().Be("guided");

        // Initial payload was stashed in DraftPayload — no Part rows created yet
        // for this run, and no junction row.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbRun = await db.WorkflowRuns.AsNoTracking().FirstAsync(r => r.Id == run.Id);
        dbRun.DraftPayload.Should().NotBeNullOrWhiteSpace();
        dbRun.DraftPayload.Should().Contain("Lifecycle Test Widget");

        var junctionCount = await db.WorkflowRunEntities
            .Where(j => j.RunId == run.Id)
            .CountAsync();
        junctionCount.Should().Be(0, "junction row is inserted at materialization, not start");

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
    public async Task PatchStep_AdvancesCurrentStep_WhenGatePasses_AndMaterializesEntity()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");
        run.EntityId.Should().BeNull();

        // basics step has all three required fields. Send them and expect
        // materialization + advance.
        var fields = JsonDocument.Parse("""{"name":"Adv Test","material":"Steel","partType":"Part"}""").RootElement;
        var step = new PatchWorkflowStepRequestModel("basics", fields);
        var response = await client.PatchAsJsonAsync($"/api/v1/workflows/{run.Id}/step", step);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<WorkflowRunResponseModel>();
        updated!.CurrentStepId.Should().Be("bom");
        updated!.EntityId.Should().NotBeNull("materialization stamps the new entity id back on the run");
        updated!.EntityId.Should().BePositive();

        // Junction row now exists.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var junction = await db.WorkflowRunEntities
            .Where(j => j.RunId == run.Id)
            .ToListAsync();
        junction.Should().HaveCount(1);
        junction[0].EntityId.Should().Be(updated.EntityId!.Value);
        junction[0].Role.Should().Be("primary");

        // Draft payload cleared after materialization.
        var dbRun = await db.WorkflowRuns.AsNoTracking().FirstAsync(r => r.Id == run.Id);
        dbRun.DraftPayload.Should().BeNull();
    }

    [Fact]
    public async Task PatchStep_MergesDraftPayload_WithStepFields()
    {
        var client = AuthenticatedClient();
        // Start with material in initial payload, then patch with name+partType.
        // Materialization should merge both halves into a single CreateDraft call.
        var initial = JsonDocument.Parse("""{"material":"Aluminum"}""").RootElement;
        var body = new StartWorkflowRunRequestModel(
            "Part", "part-assembly-guided-v1", "guided", initial);
        var startResp = await client.PostAsJsonAsync("/api/v1/workflows", body);
        var run = (await startResp.Content.ReadFromJsonAsync<WorkflowRunResponseModel>())!;
        run.EntityId.Should().BeNull();

        var fields = JsonDocument.Parse("""{"name":"Merged","partType":"Part"}""").RootElement;
        var resp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel("basics", fields));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await resp.Content.ReadFromJsonAsync<WorkflowRunResponseModel>();
        updated!.EntityId.Should().NotBeNull();
        updated!.CurrentStepId.Should().Be("bom", "all three basics fields present after merge → gate passes");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var part = await db.Parts.FirstAsync(p => p.Id == updated.EntityId!.Value);
        part.Name.Should().Be("Merged");
        part.Material.Should().Be("Aluminum");
        part.PartType.Should().Be(PartType.Part);
    }

    [Fact]
    public async Task PatchStep_RejectsNonFirstStep_WhenEntityNotMaterialized()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, """{"name":"Skipper"}""");
        run.EntityId.Should().BeNull();

        // bom step requires a materialized part — patch should 409 before
        // ever invoking the BOM applier.
        var fields = JsonDocument.Parse("""{"someBomField":"value"}""").RootElement;
        var resp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel("bom", fields));
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PatchStep_FirstStep_RequiresName_OrReturns400()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        // Patch basics with no name — materialization must reject with 400.
        var fields = JsonDocument.Parse("""{"partType":"Part","material":"Steel"}""").RootElement;
        var resp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel("basics", fields));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbRun = await db.WorkflowRuns.AsNoTracking().FirstAsync(r => r.Id == run.Id);
        dbRun.EntityId.Should().BeNull("materialization failed → no part row, no stamped id");
    }

    [Fact]
    public async Task PatchStep_DoesNotAdvance_WhenGateFails()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        // Provide name + partType but no material → hasBasics fails (material
        // gate not satisfied). Materialization succeeds (name present), but
        // pointer stays on basics.
        var fields = JsonDocument.Parse("""{"name":"Only name","partType":"Part"}""").RootElement;
        var step = new PatchWorkflowStepRequestModel("basics", fields);
        var response = await client.PatchAsJsonAsync($"/api/v1/workflows/{run.Id}/step", step);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<WorkflowRunResponseModel>();
        updated!.CurrentStepId.Should().Be("basics");
        updated!.EntityId.Should().NotBeNull("entity is materialized even if gate fails — name was present");
    }

    [Fact]
    public async Task Jump_BackToCompletedStep_Allowed()
    {
        var client = AuthenticatedClient();
        var initial = JsonDocument.Parse("""{"name":"Jumper","partType":"Part","material":"Steel"}""").RootElement;
        var run = await StartRunAsync(client, initial.GetRawText());

        // Advance past basics with a gate-passing patch (also materializes).
        await client.PatchAsJsonAsync($"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel("basics", initial));

        // Jump back to basics.
        var jumpResp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/jump",
            new JumpWorkflowRequestModel("basics"));
        jumpResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Jump_ForwardBeforeMaterialization_409()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, """{"name":"Skipper"}"""); // no patch yet → no entity

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
    public async Task Abandon_BeforeMaterialization_LeavesNoOrphanEntity()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, """{"name":"WillAbandon"}""");
        run.EntityId.Should().BeNull();

        var resp = await client.PostAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/abandon",
            new AbandonWorkflowRequestModel("user"));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // No Part row was ever created for this run — verify by part name uniqueness.
        var orphan = await db.Parts
            .IgnoreQueryFilters()
            .AnyAsync(p => p.Name == "WillAbandon");
        orphan.Should().BeFalse("nothing was materialized → nothing to soft-delete");

        var refreshed = await db.WorkflowRuns.AsNoTracking().FirstAsync(r => r.Id == run.Id);
        refreshed.AbandonedAt.Should().NotBeNull();
        refreshed.AbandonedReason.Should().Be("user");
    }

    [Fact]
    public async Task Abandon_AfterMaterialization_SoftDeletesDraftEntity()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        // Patch basics so the entity materializes.
        var fields = JsonDocument.Parse("""{"name":"Materialized","material":"Steel","partType":"Part"}""").RootElement;
        var patchResp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel("basics", fields));
        var afterPatch = (await patchResp.Content.ReadFromJsonAsync<WorkflowRunResponseModel>())!;
        afterPatch.EntityId.Should().NotBeNull();
        var entityId = afterPatch.EntityId!.Value;

        var resp = await client.PostAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/abandon",
            new AbandonWorkflowRequestModel("user"));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var part = await db.Parts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == entityId);
        part!.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Complete_FailsBeforeMaterialization_WithReadinessEnvelope()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, """{"name":"Incomplete"}""");
        run.EntityId.Should().BeNull();

        var resp = await client.PostAsync($"/api/v1/workflows/{run.Id}/complete", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("missing").GetArrayLength().Should().BeGreaterThan(0);
        doc.RootElement.GetProperty("code").GetString().Should().Be("workflow-readiness-missing");
    }

    [Fact]
    public async Task Complete_FailsWhenReadinessUnsatisfied_AfterMaterialization()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        // Materialize via basics patch with name only — gate fails (no material).
        await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel(
                "basics",
                JsonDocument.Parse("""{"name":"OnlyName","partType":"Part"}""").RootElement));

        var resp = await client.PostAsync($"/api/v1/workflows/{run.Id}/complete", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("missing").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PromoteStatus_FailsWhenReadinessUnsatisfied()
    {
        var client = AuthenticatedClient();
        var run = await StartRunAsync(client, "{}");

        // Materialize with insufficient data so the part exists but isn't ready.
        var patchResp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel(
                "basics",
                JsonDocument.Parse("""{"name":"NotReady","partType":"Part"}""").RootElement));
        var afterPatch = (await patchResp.Content.ReadFromJsonAsync<WorkflowRunResponseModel>())!;

        var resp = await client.PostAsJsonAsync(
            $"/api/v1/parts/{afterPatch.EntityId}/promote-status",
            new PromoteEntityStatusRequestModel("Active"));
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PromoteStatus_SucceedsWhenAllValidatorsPass()
    {
        var client = AuthenticatedClient();
        var initial = JsonDocument.Parse(
            """{"name":"Ready","partType":"Part","material":"Steel","manualCostOverride":12.5}""").RootElement;
        var run = await StartRunAsync(client, initial.GetRawText());

        // Materialize via basics patch so the entity exists.
        var patchResp = await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel("basics", initial));
        var afterPatch = (await patchResp.Content.ReadFromJsonAsync<WorkflowRunResponseModel>())!;
        var entityId = afterPatch.EntityId!.Value;

        // Add BOM + Operation rows directly (these are usually edited via
        // their dedicated endpoints in the workflow's BOM/Routing steps).
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var component = new Part
            {
                PartNumber = $"TC-{Guid.NewGuid():N}"[..16],
                Name = "Component",
                PartType = PartType.Part,
                Status = PartStatus.Active,
            };
            db.Parts.Add(component);
            await db.SaveChangesAsync();
            db.BOMEntries.Add(new BOMEntry
            {
                ParentPartId = entityId,
                ChildPartId = component.Id,
                Quantity = 1,
            });
            db.Operations.Add(new Operation
            {
                PartId = entityId,
                StepNumber = 10,
                Title = "OP10",
            });
            await db.SaveChangesAsync();
        }

        var resp = await client.PostAsJsonAsync(
            $"/api/v1/parts/{entityId}/promote-status",
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
    public async Task Complete_MissingValidators_ScopedToRunDefinition()
    {
        var client = AuthenticatedClient();
        // Raw-material express workflow only gates on hasBasics + hasCost.
        // Start one with a name (so basics partially fills) but no cost
        // override — the missing list should NOT include hasBom or hasRouting
        // because they aren't gates of this definition.
        var initial = JsonDocument.Parse("""{"name":"ScopeTest","partType":"RawMaterial","material":"Steel"}""").RootElement;
        var body = new StartWorkflowRunRequestModel(
            "Part", "part-raw-material-express-v1", "express", initial);
        var startResp = await client.PostAsJsonAsync("/api/v1/workflows", body);
        startResp.EnsureSuccessStatusCode();
        var run = (await startResp.Content.ReadFromJsonAsync<WorkflowRunResponseModel>())!;

        // Materialize via a step patch (workflow's only step is "all").
        await client.PatchAsJsonAsync(
            $"/api/v1/workflows/{run.Id}/step",
            new PatchWorkflowStepRequestModel("all", initial));

        var resp = await client.PostAsync($"/api/v1/workflows/{run.Id}/complete", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var jsonBody = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonBody);
        var missingIds = doc.RootElement.GetProperty("missing")
            .EnumerateArray()
            .Select(m => m.GetProperty("validatorId").GetString())
            .ToList();
        missingIds.Should().Contain("hasCost");
        missingIds.Should().NotContain("hasBom");
        missingIds.Should().NotContain("hasRouting");
    }

    [Fact]
    public async Task Complete_BeforeMaterialization_MissingScopedToRunDefinition()
    {
        var client = AuthenticatedClient();
        // Same as above but without a step patch — the entity isn't created
        // yet. The pre-materialization branch should also scope to the run's
        // gates instead of dumping the full entity-type validator catalog.
        var initial = JsonDocument.Parse("""{"name":"NoMatHere"}""").RootElement;
        var body = new StartWorkflowRunRequestModel(
            "Part", "part-raw-material-express-v1", "express", initial);
        var startResp = await client.PostAsJsonAsync("/api/v1/workflows", body);
        var run = (await startResp.Content.ReadFromJsonAsync<WorkflowRunResponseModel>())!;
        run.EntityId.Should().BeNull();

        var resp = await client.PostAsync($"/api/v1/workflows/{run.Id}/complete", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var jsonBody = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonBody);
        var missingIds = doc.RootElement.GetProperty("missing")
            .EnumerateArray()
            .Select(m => m.GetProperty("validatorId").GetString())
            .ToList();
        missingIds.Should().BeEquivalentTo(new[] { "hasBasics", "hasCost" });
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
        var initial = JsonDocument.Parse("""{"name":"Auditable","partType":"Part","material":"Steel"}""").RootElement;
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
