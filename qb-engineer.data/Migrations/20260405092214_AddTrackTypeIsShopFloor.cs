using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackTypeIsShopFloor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_shop_floor",
                table: "track_types",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // R&D/Tooling is not a shop floor track
            migrationBuilder.Sql("UPDATE track_types SET is_shop_floor = false WHERE code = 'rnd';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_shop_floor",
                table: "track_types");
        }
    }
}
