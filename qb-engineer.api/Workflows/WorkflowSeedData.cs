namespace QBEngineer.Api.Workflows;

/// <summary>
/// Workflow Pattern Phase 3 — Seed payload for the Part entity. The seed
/// strings are inlined here (instead of loaded from a resource file) so the
/// seeder can run without filesystem access. Mirrors the worked example in
/// <c>docs/workflow-pattern.md</c>.
///
/// <para>Pillar 6 (per the audit at <c>phase-4-output/part-type-field-relevance.md §5</c>)
/// authors 14 combo-specific definitions covering the 11 viable
/// (procurement × inventory) combos. Each combo carries both an express
/// template and a guided steps list per resolved decision #10 (user-pickable
/// mode at runtime, with a per-combo recommended default).</para>
///
/// <para>Step <c>componentName</c> values reference existing Angular step
/// components today. Combos that reference <c>PartSourcingStepComponent</c>,
/// <c>PartManufacturerStepComponent</c>, <c>PartInventoryStepComponent</c>,
/// <c>PartQualityStepComponent</c>, <c>PartShippingStepComponent</c>,
/// <c>PartSourcePartStepComponent</c>, <c>PartVendorStepComponent</c>,
/// <c>PartToolAssetStepComponent</c>, <c>PartFlagsStepComponent</c>, or
/// <c>PartSalesHooksStepComponent</c> are placeholder names — Pillar 4 will
/// build those step components and adopt the steps without re-seeding.</para>
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
            // Pre-beta: the legacy `partType` + `material` text fields were
            // dropped along with the single-axis enum. Basics now requires the
            // three orthogonal axes' answer (procurement source + inventory
            // class) to be present — they are emitted by the fork dialog
            // before the workflow even starts, so this gate is always
            // satisfied when name is set.
            Predicate: """
            {"type":"all","of":[
              {"type":"fieldPresent","field":"name"},
              {"type":"fieldPresent","field":"procurementSource"},
              {"type":"fieldPresent","field":"inventoryClass"}
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
        // -------- Buy combos (B1-B6) --------
        new(
            DefinitionId: "part-buy-raw-v1",
            EntityType: "Part",
            DefaultMode: "express",
            StepsJson: BuildBuyRawStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-buy-component-v1",
            EntityType: "Part",
            DefaultMode: "express",
            StepsJson: BuildBuyComponentStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-buy-subassembly-v1",
            EntityType: "Part",
            DefaultMode: "guided",
            StepsJson: BuildBuySubassemblyStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-buy-finishedgood-v1",
            EntityType: "Part",
            DefaultMode: "express",
            StepsJson: BuildBuyFinishedGoodStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-buy-consumable-v1",
            EntityType: "Part",
            DefaultMode: "express",
            StepsJson: BuildBuyConsumableStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-buy-tool-v1",
            EntityType: "Part",
            DefaultMode: "express",
            StepsJson: BuildBuyToolStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),

        // -------- Make combos (M1-M4) --------
        new(
            DefinitionId: "part-make-component-v1",
            EntityType: "Part",
            DefaultMode: "guided",
            StepsJson: BuildMakeComponentStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-make-subassembly-v1",
            EntityType: "Part",
            DefaultMode: "guided",
            StepsJson: BuildMakeSubassemblyStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-make-finishedgood-v1",
            EntityType: "Part",
            DefaultMode: "guided",
            StepsJson: BuildMakeFinishedGoodStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-make-tool-v1",
            EntityType: "Part",
            DefaultMode: "guided",
            StepsJson: BuildMakeToolStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),

        // -------- Subcontract combos (S1-S2) --------
        new(
            DefinitionId: "part-subcontract-component-v1",
            EntityType: "Part",
            DefaultMode: "guided",
            StepsJson: BuildSubcontractComponentStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-subcontract-subassembly-v1",
            EntityType: "Part",
            DefaultMode: "guided",
            StepsJson: BuildSubcontractSubassemblyStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),

        // -------- Phantom combos (P1, P3) --------
        new(
            DefinitionId: "part-phantom-subassembly-v1",
            EntityType: "Part",
            DefaultMode: "express",
            StepsJson: BuildPhantomSubassemblyStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
        new(
            DefinitionId: "part-phantom-finishedgood-v1",
            EntityType: "Part",
            DefaultMode: "express",
            StepsJson: BuildPhantomFinishedGoodStepsJson(),
            ExpressTemplateComponent: "PartExpressFormComponent"),
    ];

    // ---------- Buy combo step builders ----------

    // B1 — Buy + Raw. Audit §5.B1: 4 guided steps (Identity, Sourcing,
    // Inventory, Quality). No BOM, no Routing. Gates on hasBasics + hasCost.
    private static string BuildBuyRawStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"sourcing","labelKey":"workflow.parts.steps.sourcing","componentName":"PartSourcingStepComponent","required":true,"completionGates":[]},
      {"id":"inventory","labelKey":"workflow.parts.steps.inventory","componentName":"PartInventoryStepComponent","required":true,"completionGates":[]},
      {"id":"quality","labelKey":"workflow.parts.steps.quality","componentName":"PartQualityStepComponent","required":false,"completionGates":[]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // B2 — Buy + Component. Audit §5.B2: 5 guided steps (Identity,
    // Manufacturer, Sourcing, Inventory, Cost). Gates on hasBasics + hasCost.
    private static string BuildBuyComponentStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"manufacturer","labelKey":"workflow.parts.steps.manufacturer","componentName":"PartManufacturerStepComponent","required":true,"completionGates":[]},
      {"id":"sourcing","labelKey":"workflow.parts.steps.sourcing","componentName":"PartSourcingStepComponent","required":true,"completionGates":[]},
      {"id":"inventory","labelKey":"workflow.parts.steps.inventory","componentName":"PartInventoryStepComponent","required":true,"completionGates":[]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // B3 — Buy + Subassembly. Audit §5.B3: same as B2 + Quality step. Gates
    // on hasBasics + hasCost. Manufacturer step gathers physical attributes
    // (WeightEach, Dimensions) too.
    private static string BuildBuySubassemblyStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"manufacturer","labelKey":"workflow.parts.steps.manufacturer","componentName":"PartManufacturerStepComponent","required":true,"completionGates":[]},
      {"id":"sourcing","labelKey":"workflow.parts.steps.sourcing","componentName":"PartSourcingStepComponent","required":true,"completionGates":[]},
      {"id":"inventory","labelKey":"workflow.parts.steps.inventory","componentName":"PartInventoryStepComponent","required":true,"completionGates":[]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]},
      {"id":"quality","labelKey":"workflow.parts.steps.quality","componentName":"PartQualityStepComponent","required":true,"completionGates":[]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // B4 — Buy + FinishedGood (resold). Audit §5.B4: 5 guided steps.
    // Adds Shipping step (WeightEach, Dimensions). Gates on hasBasics + hasCost.
    private static string BuildBuyFinishedGoodStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"manufacturer","labelKey":"workflow.parts.steps.manufacturer","componentName":"PartManufacturerStepComponent","required":true,"completionGates":[]},
      {"id":"sourcing","labelKey":"workflow.parts.steps.sourcing","componentName":"PartSourcingStepComponent","required":true,"completionGates":[]},
      {"id":"inventory","labelKey":"workflow.parts.steps.inventory","componentName":"PartInventoryStepComponent","required":true,"completionGates":[]},
      {"id":"shipping","labelKey":"workflow.parts.steps.shipping","componentName":"PartShippingStepComponent","required":true,"completionGates":[]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // B5 — Buy + Consumable. Audit §5.B5: express-only, 5 sections. We still
    // expose a "sections" express form here. Mode default is express; when
    // users opt into guided we collapse the sections into individual steps
    // for parity. Gates: hasBasics + hasCost.
    private static string BuildBuyConsumableStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"sourcing","labelKey":"workflow.parts.steps.sourcing","componentName":"PartSourcingStepComponent","required":true,"completionGates":[]},
      {"id":"inventory","labelKey":"workflow.parts.steps.inventory","componentName":"PartInventoryStepComponent","required":true,"completionGates":[]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // B6 — Buy + Tool. Audit §5.B6: same as B5 plus a Tool section
    // (ToolingAssetId, calibration). Gates: hasBasics + hasCost.
    private static string BuildBuyToolStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"toolAsset","labelKey":"workflow.parts.steps.toolAsset","componentName":"PartToolAssetStepComponent","required":true,"completionGates":[]},
      {"id":"sourcing","labelKey":"workflow.parts.steps.sourcing","componentName":"PartSourcingStepComponent","required":true,"completionGates":[]},
      {"id":"inventory","labelKey":"workflow.parts.steps.inventory","componentName":"PartInventoryStepComponent","required":true,"completionGates":[]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // ---------- Make combo step builders ----------

    // M1 — Make + Component. Audit §5.M1: 5 guided steps. BOM is optional
    // (single-piece fab). Routing required for promotion to Active. Gates:
    // hasBasics + hasRouting + hasCost. (BOM not gated since some Make+Component
    // parts are single-piece fabricated from a single raw stock.)
    private static string BuildMakeComponentStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"manufacturing","labelKey":"workflow.parts.steps.manufacturing","componentName":"PartInventoryStepComponent","required":true,"completionGates":[]},
      {"id":"bom","labelKey":"workflow.parts.steps.bom","componentName":"PartBomStepComponent","required":false,"completionGates":[]},
      {"id":"routing","labelKey":"workflow.parts.steps.routing","componentName":"PartRoutingStepComponent","required":true,"completionGates":["hasRouting"]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]},
      {"id":"alternates","labelKey":"workflow.parts.steps.alternates","componentName":"PartAlternatesStepComponent","required":false,"completionGates":[]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // M2 — Make + Subassembly. Audit §5.M2: same as M1 with BOM required +
    // optional Quality. Gates: hasBasics + hasBom + hasRouting + hasCost.
    private static string BuildMakeSubassemblyStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"bom","labelKey":"workflow.parts.steps.bom","componentName":"PartBomStepComponent","required":true,"completionGates":["hasBom"]},
      {"id":"routing","labelKey":"workflow.parts.steps.routing","componentName":"PartRoutingStepComponent","required":true,"completionGates":["hasRouting"]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]},
      {"id":"quality","labelKey":"workflow.parts.steps.quality","componentName":"PartQualityStepComponent","required":false,"completionGates":[]},
      {"id":"alternates","labelKey":"workflow.parts.steps.alternates","componentName":"PartAlternatesStepComponent","required":false,"completionGates":[]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // M3 — Make + FinishedGood. Audit §5.M3: same as M2 + Sales & Shipping
    // step. Gates: hasBasics + hasBom + hasRouting + hasCost.
    private static string BuildMakeFinishedGoodStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"bom","labelKey":"workflow.parts.steps.bom","componentName":"PartBomStepComponent","required":true,"completionGates":["hasBom"]},
      {"id":"routing","labelKey":"workflow.parts.steps.routing","componentName":"PartRoutingStepComponent","required":true,"completionGates":["hasRouting"]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]},
      {"id":"shipping","labelKey":"workflow.parts.steps.shipping","componentName":"PartShippingStepComponent","required":true,"completionGates":[]},
      {"id":"quality","labelKey":"workflow.parts.steps.quality","componentName":"PartQualityStepComponent","required":false,"completionGates":[]},
      {"id":"alternates","labelKey":"workflow.parts.steps.alternates","componentName":"PartAlternatesStepComponent","required":false,"completionGates":[]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // M4 — Make + Tool. Audit §5.M4: 4 guided steps. No inventory thresholds
    // (tools aren't reordered as parts). Gates: hasBasics + hasBom +
    // hasRouting. Cost is not gated for tool builds — tooling cost flows via
    // the asset record, not the part.
    private static string BuildMakeToolStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"toolAsset","labelKey":"workflow.parts.steps.toolAsset","componentName":"PartToolAssetStepComponent","required":true,"completionGates":[]},
      {"id":"bom","labelKey":"workflow.parts.steps.bom","componentName":"PartBomStepComponent","required":true,"completionGates":["hasBom"]},
      {"id":"routing","labelKey":"workflow.parts.steps.routing","componentName":"PartRoutingStepComponent","required":true,"completionGates":["hasRouting"]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // ---------- Subcontract combo step builders ----------

    // S1 — Subcontract + Component. Audit §5.S1: 4 guided steps. SourcePart
    // pointer + Vendor + Quality. Gates: hasBasics + hasCost. (No internal
    // routing — the subcontractor owns the operations; we represent that
    // via the source-part pointer rather than an Operations list, hence
    // hasRouting is not gated here.)
    private static string BuildSubcontractComponentStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"sourcePart","labelKey":"workflow.parts.steps.sourcePart","componentName":"PartSourcePartStepComponent","required":true,"completionGates":[]},
      {"id":"vendor","labelKey":"workflow.parts.steps.vendor","componentName":"PartVendorStepComponent","required":true,"completionGates":[]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]},
      {"id":"quality","labelKey":"workflow.parts.steps.quality","componentName":"PartQualityStepComponent","required":true,"completionGates":[]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // S2 — Subcontract + Subassembly. Audit §5.S2: same as S1 with BOM step
    // inserted (we own the design even though vendor builds). Gates:
    // hasBasics + hasBom + hasCost.
    private static string BuildSubcontractSubassemblyStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"sourcePart","labelKey":"workflow.parts.steps.sourcePart","componentName":"PartSourcePartStepComponent","required":true,"completionGates":[]},
      {"id":"bom","labelKey":"workflow.parts.steps.bom","componentName":"PartBomStepComponent","required":true,"completionGates":["hasBom"]},
      {"id":"vendor","labelKey":"workflow.parts.steps.vendor","componentName":"PartVendorStepComponent","required":true,"completionGates":[]},
      {"id":"costing","labelKey":"workflow.parts.steps.costing","componentName":"PartCostingStepComponent","required":true,"completionGates":["hasCost"]},
      {"id":"quality","labelKey":"workflow.parts.steps.quality","componentName":"PartQualityStepComponent","required":true,"completionGates":[]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // ---------- Phantom combo step builders ----------

    // P1 — Phantom + Subassembly. Audit §5.P1: express-only short form.
    // Identity + BOM + Flags. No sourcing, inventory, cost, or quality —
    // phantoms are never stocked. Gates: hasBasics + hasBom.
    private static string BuildPhantomSubassemblyStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"bom","labelKey":"workflow.parts.steps.bom","componentName":"PartBomStepComponent","required":true,"completionGates":["hasBom"]},
      {"id":"flags","labelKey":"workflow.parts.steps.flags","componentName":"PartFlagsStepComponent","required":false,"completionGates":[]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // P3 — Phantom + FinishedGood (configure-to-order parent). Audit §5.P3:
    // 3-step guided, recommended express. No sourcing, inventory thresholds,
    // MRP, quality, or shipping — parent is never stocked. Gates: hasBasics
    // + hasBom.
    private static string BuildPhantomFinishedGoodStepsJson() => """
    [
      {"id":"basics","labelKey":"workflow.parts.steps.basics","componentName":"PartBasicsStepComponent","required":true,"completionGates":["hasBasics"]},
      {"id":"bom","labelKey":"workflow.parts.steps.bom","componentName":"PartBomStepComponent","required":true,"completionGates":["hasBom"]},
      {"id":"salesHooks","labelKey":"workflow.parts.steps.salesHooks","componentName":"PartSalesHooksStepComponent","required":false,"completionGates":[]}
    ]
    """.Replace("\r", "").Replace("\n", "").Replace("  ", "");

    // (Pre-beta: the legacy `part-assembly-guided-v1` and
    // `part-raw-material-express-v1` transitional alias builders were
    // retired along with the single-axis fork dialog. New runs always go
    // through one of the 14 canonical combo definitions above.)
}
