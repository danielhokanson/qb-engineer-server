using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSeedDataToReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_seed_data",
                table: "reference_data",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Backfill: mark all existing reference data as seed data
            migrationBuilder.Sql("UPDATE reference_data SET is_seed_data = true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_seed_data",
                table: "reference_data");
        }
    }
}
