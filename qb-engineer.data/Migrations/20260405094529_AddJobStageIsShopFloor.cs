using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobStageIsShopFloor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_shop_floor",
                table: "job_stages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Production track: physical work stages
            migrationBuilder.Sql(@"
                UPDATE job_stages SET is_shop_floor = true
                WHERE code IN ('materials_ordered', 'materials_received', 'in_production', 'qc_review', 'shipped');
            ");

            // Maintenance track: scheduled and active work
            migrationBuilder.Sql(@"
                UPDATE job_stages SET is_shop_floor = true
                WHERE code IN ('scheduled', 'in_progress', 'complete');
            ");

            // R&D track: hands-on stages
            migrationBuilder.Sql(@"
                UPDATE job_stages SET is_shop_floor = true
                WHERE code IN ('prototype', 'test', 'iterate');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_shop_floor",
                table: "job_stages");
        }
    }
}
