using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderManagementEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sales_order_line_id",
                table: "jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "customer_addresses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_type = table.Column<int>(type: "integer", nullable: false),
                    line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_addresses_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payment_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    method = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reference_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_payments_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "price_lists",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_lists", x => x.id);
                    table.ForeignKey(
                        name: "fk_price_lists_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "quotes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quote_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    shipping_address_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    sent_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    accepted_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    tax_rate = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quotes", x => x.id);
                    table.ForeignKey(
                        name: "fk_quotes_customer_addresses_shipping_address_id",
                        column: x => x.shipping_address_id,
                        principalTable: "customer_addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_quotes_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recurring_orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    shipping_address_id = table.Column<int>(type: "integer", nullable: true),
                    interval_days = table.Column<int>(type: "integer", nullable: false),
                    next_generation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_generated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recurring_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_recurring_orders_customer_addresses_shipping_address_id",
                        column: x => x.shipping_address_id,
                        principalTable: "customer_addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_recurring_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "price_list_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    price_list_id = table.Column<int>(type: "integer", nullable: false),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    min_quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_list_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_price_list_entries_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_price_list_entries_price_lists_price_list_id",
                        column: x => x.price_list_id,
                        principalTable: "price_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quote_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quote_id = table.Column<int>(type: "integer", nullable: false),
                    part_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quote_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_quote_lines_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_quote_lines_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "quotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    quote_id = table.Column<int>(type: "integer", nullable: true),
                    shipping_address_id = table.Column<int>(type: "integer", nullable: true),
                    billing_address_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    credit_terms = table.Column<int>(type: "integer", nullable: true),
                    confirmed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    requested_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    customer_po = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    tax_rate = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_sales_orders_customer_addresses_billing_address_id",
                        column: x => x.billing_address_id,
                        principalTable: "customer_addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_sales_orders_customer_addresses_shipping_address_id",
                        column: x => x.shipping_address_id,
                        principalTable: "customer_addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_sales_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sales_orders_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "quotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "recurring_order_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recurring_order_id = table.Column<int>(type: "integer", nullable: false),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recurring_order_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_recurring_order_lines_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_recurring_order_lines_recurring_orders_recurring_order_id",
                        column: x => x.recurring_order_id,
                        principalTable: "recurring_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_order_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sales_order_id = table.Column<int>(type: "integer", nullable: false),
                    part_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    shipped_quantity = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_order_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_sales_order_lines_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_sales_order_lines_sales_orders_sales_order_id",
                        column: x => x.sales_order_id,
                        principalTable: "sales_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shipment_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sales_order_id = table.Column<int>(type: "integer", nullable: false),
                    shipping_address_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    carrier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tracking_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipped_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    shipping_cost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    weight = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipments", x => x.id);
                    table.ForeignKey(
                        name: "fk_shipments_customer_addresses_shipping_address_id",
                        column: x => x.shipping_address_id,
                        principalTable: "customer_addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_shipments_sales_orders_sales_order_id",
                        column: x => x.sales_order_id,
                        principalTable: "sales_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoice_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    sales_order_id = table.Column<int>(type: "integer", nullable: true),
                    shipment_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    invoice_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    credit_terms = table.Column<int>(type: "integer", nullable: true),
                    tax_rate = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoices__sales_orders_sales_order_id",
                        column: x => x.sales_order_id,
                        principalTable: "sales_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_invoices__shipments_shipment_id",
                        column: x => x.shipment_id,
                        principalTable: "shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_invoices_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shipment_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shipment_id = table.Column<int>(type: "integer", nullable: false),
                    sales_order_line_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipment_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_shipment_lines_sales_order_lines_sales_order_line_id",
                        column: x => x.sales_order_line_id,
                        principalTable: "sales_order_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shipment_lines_shipments_shipment_id",
                        column: x => x.shipment_id,
                        principalTable: "shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoice_id = table.Column<int>(type: "integer", nullable: false),
                    part_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoice_lines__parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_invoice_lines_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_applications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payment_id = table.Column<int>(type: "integer", nullable: false),
                    invoice_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_applications_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payment_applications_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_jobs_sales_order_line_id",
                table: "jobs",
                column: "sales_order_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_addresses_customer_id",
                table: "customer_addresses",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoice_lines_invoice_id",
                table: "invoice_lines",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoice_lines_part_id",
                table: "invoice_lines",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_customer_id",
                table: "invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_invoice_number",
                table: "invoices",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_sales_order_id",
                table: "invoices",
                column: "sales_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_shipment_id",
                table: "invoices",
                column: "shipment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_status",
                table: "invoices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_payment_applications_invoice_id",
                table: "payment_applications",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_applications_payment_id",
                table: "payment_applications",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_customer_id",
                table: "payments",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_method",
                table: "payments",
                column: "method");

            migrationBuilder.CreateIndex(
                name: "ix_payments_payment_number",
                table: "payments",
                column: "payment_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_price_list_entries_part_id",
                table: "price_list_entries",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_price_list_entries_price_list_id",
                table: "price_list_entries",
                column: "price_list_id");

            migrationBuilder.CreateIndex(
                name: "ix_price_list_entries_price_list_id_part_id_min_quantity",
                table: "price_list_entries",
                columns: new[] { "price_list_id", "part_id", "min_quantity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_price_lists_customer_id",
                table: "price_lists",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_quote_lines_part_id",
                table: "quote_lines",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_quote_lines_quote_id",
                table: "quote_lines",
                column: "quote_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_customer_id",
                table: "quotes",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_quote_number",
                table: "quotes",
                column: "quote_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quotes_shipping_address_id",
                table: "quotes",
                column: "shipping_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_status",
                table: "quotes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_order_lines_part_id",
                table: "recurring_order_lines",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_order_lines_recurring_order_id",
                table: "recurring_order_lines",
                column: "recurring_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_orders_customer_id",
                table: "recurring_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_orders_next_generation_date",
                table: "recurring_orders",
                column: "next_generation_date");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_orders_shipping_address_id",
                table: "recurring_orders",
                column: "shipping_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_order_lines_part_id",
                table: "sales_order_lines",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_order_lines_sales_order_id",
                table: "sales_order_lines",
                column: "sales_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_orders_billing_address_id",
                table: "sales_orders",
                column: "billing_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_orders_customer_id",
                table: "sales_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_orders_order_number",
                table: "sales_orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_orders_quote_id",
                table: "sales_orders",
                column: "quote_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_orders_shipping_address_id",
                table: "sales_orders",
                column: "shipping_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_orders_status",
                table: "sales_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_shipment_lines_sales_order_line_id",
                table: "shipment_lines",
                column: "sales_order_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipment_lines_shipment_id",
                table: "shipment_lines",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_sales_order_id",
                table: "shipments",
                column: "sales_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_shipment_number",
                table: "shipments",
                column: "shipment_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipments_shipping_address_id",
                table: "shipments",
                column: "shipping_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_status",
                table: "shipments",
                column: "status");

            migrationBuilder.AddForeignKey(
                name: "fk_jobs__sales_order_lines_sales_order_line_id",
                table: "jobs",
                column: "sales_order_line_id",
                principalTable: "sales_order_lines",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_jobs__sales_order_lines_sales_order_line_id",
                table: "jobs");

            migrationBuilder.DropTable(
                name: "invoice_lines");

            migrationBuilder.DropTable(
                name: "payment_applications");

            migrationBuilder.DropTable(
                name: "price_list_entries");

            migrationBuilder.DropTable(
                name: "quote_lines");

            migrationBuilder.DropTable(
                name: "recurring_order_lines");

            migrationBuilder.DropTable(
                name: "shipment_lines");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "price_lists");

            migrationBuilder.DropTable(
                name: "recurring_orders");

            migrationBuilder.DropTable(
                name: "sales_order_lines");

            migrationBuilder.DropTable(
                name: "shipments");

            migrationBuilder.DropTable(
                name: "sales_orders");

            migrationBuilder.DropTable(
                name: "quotes");

            migrationBuilder.DropTable(
                name: "customer_addresses");

            migrationBuilder.DropIndex(
                name: "ix_jobs_sales_order_line_id",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "sales_order_line_id",
                table: "jobs");
        }
    }
}
