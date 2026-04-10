using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClockEventTypeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "event_type_code",
                table: "clock_events",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Backfill EventTypeCode from existing enum column values
            migrationBuilder.Sql("""
                UPDATE clock_events SET event_type_code = CASE event_type
                    WHEN 0 THEN 'ClockIn'
                    WHEN 1 THEN 'ClockOut'
                    WHEN 2 THEN 'BreakStart'
                    WHEN 3 THEN 'BreakEnd'
                    WHEN 4 THEN 'LunchStart'
                    WHEN 5 THEN 'LunchEnd'
                    ELSE 'ClockIn'
                END
                WHERE event_type_code = '' OR event_type_code IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_clock_events_event_type_code",
                table: "clock_events",
                column: "event_type_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_clock_events_event_type_code",
                table: "clock_events");

            migrationBuilder.DropColumn(
                name: "event_type_code",
                table: "clock_events");
        }
    }
}
