using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationMaterialAndSelfRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "referenced_operation_id",
                table: "operations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "operation_materials",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operation_id = table.Column<int>(type: "integer", nullable: false),
                    bom_entry_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operation_materials", x => x.id);
                    table.ForeignKey(
                        name: "fk_operation_materials_bomentries_bom_entry_id",
                        column: x => x.bom_entry_id,
                        principalTable: "bomentries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_operation_materials_operations_operation_id",
                        column: x => x.operation_id,
                        principalTable: "operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_operations_referenced_operation_id",
                table: "operations",
                column: "referenced_operation_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_materials_bom_entry_id",
                table: "operation_materials",
                column: "bom_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_materials_operation_id",
                table: "operation_materials",
                column: "operation_id");

            migrationBuilder.AddForeignKey(
                name: "fk_operations_operations_referenced_operation_id",
                table: "operations",
                column: "referenced_operation_id",
                principalTable: "operations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_operations_operations_referenced_operation_id",
                table: "operations");

            migrationBuilder.DropTable(
                name: "operation_materials");

            migrationBuilder.DropIndex(
                name: "ix_operations_referenced_operation_id",
                table: "operations");

            migrationBuilder.DropColumn(
                name: "referenced_operation_id",
                table: "operations");
        }
    }
}
