using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobCoverPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cover_photo_file_id",
                table: "jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_jobs_cover_photo_file_id",
                table: "jobs",
                column: "cover_photo_file_id");

            migrationBuilder.AddForeignKey(
                name: "fk_jobs_file_attachments_cover_photo_file_id",
                table: "jobs",
                column: "cover_photo_file_id",
                principalTable: "file_attachments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_jobs_file_attachments_cover_photo_file_id",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "ix_jobs_cover_photo_file_id",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "cover_photo_file_id",
                table: "jobs");
        }
    }
}
