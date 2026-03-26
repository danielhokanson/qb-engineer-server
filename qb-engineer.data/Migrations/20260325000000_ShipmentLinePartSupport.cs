using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class ShipmentLinePartSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make sales_order_line_id nullable
            migrationBuilder.AlterColumn<int>(
                name: "sales_order_line_id",
                table: "shipment_lines",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            // Add part_id FK column
            migrationBuilder.AddColumn<int>(
                name: "part_id",
                table: "shipment_lines",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipment_lines_part_id",
                table: "shipment_lines",
                column: "part_id");

            migrationBuilder.AddForeignKey(
                name: "fk_shipment_lines_parts_part_id",
                table: "shipment_lines",
                column: "part_id",
                principalTable: "parts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_shipment_lines_parts_part_id",
                table: "shipment_lines");

            migrationBuilder.DropIndex(
                name: "ix_shipment_lines_part_id",
                table: "shipment_lines");

            migrationBuilder.DropColumn(
                name: "part_id",
                table: "shipment_lines");

            migrationBuilder.AlterColumn<int>(
                name: "sales_order_line_id",
                table: "shipment_lines",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
