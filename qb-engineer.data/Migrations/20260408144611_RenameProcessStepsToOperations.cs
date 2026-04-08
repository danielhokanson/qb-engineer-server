using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameProcessStepsToOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename table (metadata-only, instant on Postgres)
            migrationBuilder.RenameTable(
                name: "process_steps",
                newName: "operations");

            // Rename primary key constraint
            migrationBuilder.Sql(
                "ALTER INDEX pk_process_steps RENAME TO pk_operations;");

            // Rename indexes
            migrationBuilder.RenameIndex(
                name: "ix_process_steps_part_id",
                table: "operations",
                newName: "ix_operations_part_id");

            migrationBuilder.RenameIndex(
                name: "ix_process_steps_work_center_id",
                table: "operations",
                newName: "ix_operations_work_center_id");

            // Rename foreign key constraints
            migrationBuilder.Sql(
                "ALTER TABLE operations RENAME CONSTRAINT fk_process_steps_parts_part_id TO fk_operations__parts_part_id;");

            migrationBuilder.Sql(
                "ALTER TABLE operations RENAME CONSTRAINT fk_process_steps_assets_work_center_id TO fk_operations_assets_work_center_id;");

            // Update polymorphic references
            migrationBuilder.Sql(
                "UPDATE file_attachments SET entity_type = 'Operation' WHERE entity_type = 'ProcessStep';");

            migrationBuilder.Sql(
                "UPDATE activity_logs SET entity_type = 'Operation' WHERE entity_type = 'ProcessStep';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert polymorphic references
            migrationBuilder.Sql(
                "UPDATE activity_logs SET entity_type = 'ProcessStep' WHERE entity_type = 'Operation';");

            migrationBuilder.Sql(
                "UPDATE file_attachments SET entity_type = 'ProcessStep' WHERE entity_type = 'Operation';");

            // Revert foreign key constraints
            migrationBuilder.Sql(
                "ALTER TABLE operations RENAME CONSTRAINT fk_operations_assets_work_center_id TO fk_process_steps_assets_work_center_id;");

            migrationBuilder.Sql(
                "ALTER TABLE operations RENAME CONSTRAINT fk_operations__parts_part_id TO fk_process_steps_parts_part_id;");

            // Revert indexes
            migrationBuilder.RenameIndex(
                name: "ix_operations_work_center_id",
                table: "operations",
                newName: "ix_process_steps_work_center_id");

            migrationBuilder.RenameIndex(
                name: "ix_operations_part_id",
                table: "operations",
                newName: "ix_process_steps_part_id");

            // Revert primary key
            migrationBuilder.Sql(
                "ALTER INDEX pk_operations RENAME TO pk_process_steps;");

            // Revert table rename
            migrationBuilder.RenameTable(
                name: "operations",
                newName: "process_steps");
        }
    }
}
