using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualComparisonToFormDefinitionVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "visual_comparison_json",
                table: "form_definition_versions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "visual_comparison_passed",
                table: "form_definition_versions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "visual_similarity_score",
                table: "form_definition_versions",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "visual_comparison_json",
                table: "form_definition_versions");

            migrationBuilder.DropColumn(
                name: "visual_comparison_passed",
                table: "form_definition_versions");

            migrationBuilder.DropColumn(
                name: "visual_similarity_score",
                table: "form_definition_versions");
        }
    }
}
