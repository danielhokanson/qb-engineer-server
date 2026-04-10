using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeCorrectionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "time_correction_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    time_entry_id = table.Column<int>(type: "integer", nullable: false),
                    corrected_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    original_job_id = table.Column<int>(type: "integer", nullable: true),
                    original_date = table.Column<DateOnly>(type: "date", nullable: false),
                    original_duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    original_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    original_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_time_correction_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_time_correction_logs__time_entries_time_entry_id",
                        column: x => x.time_entry_id,
                        principalTable: "time_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_time_correction_logs_corrected_by_user_id",
                table: "time_correction_logs",
                column: "corrected_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_time_correction_logs_time_entry_id",
                table: "time_correction_logs",
                column: "time_entry_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "time_correction_logs");
        }
    }
}
