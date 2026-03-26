using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixShipmentLineShadowColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_shipment_lines_sales_order_lines_sales_order_line_id1",
                table: "shipment_lines");

            migrationBuilder.DropIndex(
                name: "ix_shipment_lines_sales_order_line_id1",
                table: "shipment_lines");

            migrationBuilder.DropColumn(
                name: "sales_order_line_id1",
                table: "shipment_lines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sales_order_line_id1",
                table: "shipment_lines",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipment_lines_sales_order_line_id1",
                table: "shipment_lines",
                column: "sales_order_line_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_shipment_lines_sales_order_lines_sales_order_line_id1",
                table: "shipment_lines",
                column: "sales_order_line_id1",
                principalTable: "sales_order_lines",
                principalColumn: "id");
        }
    }
}
