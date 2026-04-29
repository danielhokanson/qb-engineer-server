namespace QBEngineer.Api.Workflows;

/// <summary>
/// Workflow Pattern Phase 3 — Seed payload for the Part entity. The seed
/// strings are inlined here (instead of loaded from a resource file) so the
/// seeder can run without filesystem access. Mirrors the worked example in
/// <c>docs/workflow-pattern.md</c>.
/// </summary>
public static class WorkflowSeedData
{
    public sealed record ValidatorSeed(
        string ValidatorId,
        string Predicate,
        string DisplayNameKey,
        string MissingMessageKey);

    public sealed record DefinitionSeed(
        string DefinitionId,
        string EntityType,
        string DefaultMode,
        string StepsJson,
        string? ExpressTemplateComponent);

    public static IReadOnlyList<ValidatorSeed> PartReadinessValidators { get; } =
    [
        new(
            ValidatorId: "hasBasics",
            Predicate: """
            {"type":"all","of":[
              {"type":"fieldPresent","field":"description"},
              {"type":"fieldPresent","field":"partType"},
              {"type":"fieldPresent","field":"material"}
            ]}
            """.Replace("\r", "").Replace("\n", "").Replace(" ", ""),
            DisplayNameKey: "validators.parts.hasBasics",
            MissingMessageKey: "validators.parts.hasBasicsMissing"),
        new(
            ValidatorId: "hasBom",
            Predicate: """{"type":"relationExists","relation":"bomEntries","minCount":1}""",
            DisplayNameKey: "validators.parts.hasBom",
            MissingMessageKey: "validators.parts.hasBomMissing"),
        new(
            ValidatorId: "hasRouting",
            Predicate: """{"type":"relationExists","relation":"operations","minCount":1}""",
            DisplayNameKey: "validators.parts.hasRouting",
            MissingMessageKey: "validators.parts.hasRoutingMissing"),
        new(
            ValidatorId: "hasCost",
            Predicate: """
            {"type":"any","of":[
              {"type":"fieldPresent","field":"manualCostOverride"},
              {"type":"fieldPresent","field":"currentCostCalculationId"}
            ]}
            """.Replace("\r", "").Replace("\n", "").Replace(" ", ""),
            DisplayNameKey: "validators.parts.hasCost",
            MissingMessageKey: "validators.parts.hasCostMissing"),
    ];

    public static IReadOnlyList<DefinitionSeed> PartWorkflowDefinitions { get; } =
    [
        new(
            DefinitionId: "part-assembly-guided-v1",
            EntityType: "Part",
            DefaultMode: "guided",
            StepsJson: BuildAssemblyGuidedStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-raw-material-express-v1",
            EntityType: "Part",
            DefaultMode: "express",
            StepsJson: BuildRawMaterialExpressStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
    ];

    private static string BuildAssemblyGuidedStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"bom","labelKey":"workflow.parts.steps.bom","componentName":"PartBomStepComponent","required":true,"completionGates":["hasBom"]},
      {"id":"routing","labelKey":"workflow.parts.steps.routing","componentName":"PartRoutingStepComponent","required":true,"completionGates":["hasRouting"]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]},
      {"id":"alternates","labelKey":"workflow.parts.steps.alternates","componentName":"PartAlternatesStepComponent","required":false,"completionGates":[]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    private static string BuildRawMaterialExpressStepsJson() => """
    [
      {"id":"all","labelKey":"workflow.parts.steps.expressForm","componentName":"PartExpressFormComponent","required":true,"completionGates":["hasBasics","hasCost"]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");
}
