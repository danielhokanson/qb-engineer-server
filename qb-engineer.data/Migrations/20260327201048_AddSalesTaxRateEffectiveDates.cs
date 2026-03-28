using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesTaxRateEffectiveDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "effective_from",
                table: "sales_tax_rates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "effective_to",
                table: "sales_tax_rates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_code",
                table: "sales_tax_rates",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_tax_rates_state_code",
                table: "sales_tax_rates",
                column: "state_code");

            migrationBuilder.CreateIndex(
                name: "ix_sales_tax_rates_state_code_effective_to",
                table: "sales_tax_rates",
                columns: new[] { "state_code", "effective_to" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sales_tax_rates_state_code",
                table: "sales_tax_rates");

            migrationBuilder.DropIndex(
                name: "ix_sales_tax_rates_state_code_effective_to",
                table: "sales_tax_rates");

            migrationBuilder.DropColumn(
                name: "effective_from",
                table: "sales_tax_rates");

            migrationBuilder.DropColumn(
                name: "effective_to",
                table: "sales_tax_rates");

            migrationBuilder.DropColumn(
                name: "state_code",
                table: "sales_tax_rates");
        }
    }
}
