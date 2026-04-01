using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReplenishmentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "lead_time_days",
                table: "parts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "reorder_quantity",
                table: "parts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "safety_stock_days",
                table: "parts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reorder_suggestions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    vendor_id = table.Column<int>(type: "integer", nullable: true),
                    current_stock = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    available_stock = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    burn_rate_daily_avg = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    burn_rate_window_days = table.Column<int>(type: "integer", nullable: false),
                    days_of_stock_remaining = table.Column<int>(type: "integer", nullable: true),
                    projected_stockout_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    incoming_po_quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    earliest_po_arrival = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    suggested_quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    approved_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resulting_purchase_order_id = table.Column<int>(type: "integer", nullable: true),
                    dismissed_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    dismissed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    dismiss_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reorder_suggestions", x => x.id);
                    table.ForeignKey(
                        name: "fk_reorder_suggestions__vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_reorder_suggestions_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reorder_suggestions_purchase_orders_resulting_purchase_orde~",
                        column: x => x.resulting_purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reorder_suggestions_part_id",
                table: "reorder_suggestions",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_reorder_suggestions_part_id_status",
                table: "reorder_suggestions",
                columns: new[] { "part_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_reorder_suggestions_resulting_purchase_order_id",
                table: "reorder_suggestions",
                column: "resulting_purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_reorder_suggestions_status",
                table: "reorder_suggestions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_reorder_suggestions_vendor_id",
                table: "reorder_suggestions",
                column: "vendor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reorder_suggestions");

            migrationBuilder.DropColumn(
                name: "lead_time_days",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "reorder_quantity",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "safety_stock_days",
                table: "parts");
        }
    }
}
