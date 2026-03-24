using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoGenerationToTrainingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "video_generation_status",
                table: "training_modules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "video_generation_error",
                table: "training_modules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_minio_key",
                table: "training_modules",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "video_generation_status",
                table: "training_modules");

            migrationBuilder.DropColumn(
                name: "video_generation_error",
                table: "training_modules");

            migrationBuilder.DropColumn(
                name: "video_minio_key",
                table: "training_modules");
        }
    }
}
