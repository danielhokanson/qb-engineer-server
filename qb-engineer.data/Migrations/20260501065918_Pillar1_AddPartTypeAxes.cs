using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class Pillar1_AddPartTypeAxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "abc_class",
                table: "parts",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "inventory_class",
                table: "parts",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Component");

            migrationBuilder.AddColumn<int>(
                name: "item_kind_id",
                table: "parts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "manufacturer_name",
                table: "parts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "manufacturer_part_number",
                table: "parts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "procurement_source",
                table: "parts",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Buy");

            migrationBuilder.AddColumn<string>(
                name: "traceability_type",
                table: "parts",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "None");

            // Pillar 1 — backfill the new axes from legacy `part_type` per the
            // audit's Section 9 mapping. Heuristic for `Part` (the catch-all):
            // BOMEntries-present ⇒ Make+Subassembly; otherwise the Buy+Component
            // default already in place. Best-effort only — no edge-case chasing
            // (Open Decision #8).
            migrationBuilder.Sql("""
                UPDATE parts SET
                    procurement_source = CASE part_type
                        WHEN 'Assembly' THEN 'Make'
                        WHEN 'RawMaterial' THEN 'Buy'
                        WHEN 'Consumable' THEN 'Buy'
                        WHEN 'Tooling' THEN CASE WHEN tooling_asset_id IS NOT NULL THEN 'Make' ELSE 'Buy' END
                        WHEN 'Fastener' THEN 'Buy'
                        WHEN 'Electronic' THEN 'Buy'
                        WHEN 'Packaging' THEN 'Buy'
                        ELSE 'Buy'
                    END,
                    inventory_class = CASE part_type
                        WHEN 'Assembly' THEN 'Subassembly'
                        WHEN 'RawMaterial' THEN 'Raw'
                        WHEN 'Consumable' THEN 'Consumable'
                        WHEN 'Tooling' THEN 'Tool'
                        WHEN 'Fastener' THEN 'Component'
                        WHEN 'Electronic' THEN 'Component'
                        WHEN 'Packaging' THEN 'Consumable'
                        ELSE 'Component'
                    END,
                    traceability_type = CASE WHEN is_serial_tracked = true THEN 'Serial' ELSE 'None' END;
            """);

            // Catch-all `Part` rows with a non-empty BOM are very likely
            // in-house subassemblies. Promote them post-default.
            migrationBuilder.Sql("""
                UPDATE parts p SET
                    procurement_source = 'Make',
                    inventory_class = 'Subassembly'
                WHERE p.part_type = 'Part'
                  AND EXISTS (SELECT 1 FROM bom_entries b WHERE b.parent_part_id = p.id AND b.deleted_at IS NULL);
            """);

            migrationBuilder.CreateIndex(
                name: "ix_parts_item_kind_id",
                table: "parts",
                column: "item_kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_parts_manufacturer_part_number",
                table: "parts",
                column: "manufacturer_part_number");

            migrationBuilder.CreateIndex(
                name: "ix_parts_procurement_source_inventory_class",
                table: "parts",
                columns: new[] { "procurement_source", "inventory_class" });

            migrationBuilder.AddForeignKey(
                name: "fk_parts__reference_data_item_kind_id",
                table: "parts",
                column: "item_kind_id",
                principalTable: "reference_data",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_parts__reference_data_item_kind_id",
                table: "parts");

            migrationBuilder.DropIndex(
                name: "ix_parts_item_kind_id",
                table: "parts");

            migrationBuilder.DropIndex(
                name: "ix_parts_manufacturer_part_number",
                table: "parts");

            migrationBuilder.DropIndex(
                name: "ix_parts_procurement_source_inventory_class",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "abc_class",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "inventory_class",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "item_kind_id",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "manufacturer_name",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "manufacturer_part_number",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "procurement_source",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "traceability_type",
                table: "parts");
        }
    }
}
