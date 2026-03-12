using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sync_queue_entries_created_at",
                table: "sync_queue_entries");

            migrationBuilder.DropIndex(
                name: "ix_sync_queue_entries_status",
                table: "sync_queue_entries");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<string>(
                name: "custom_field_definitions",
                table: "track_types",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "payload",
                table: "sync_queue_entries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "operation",
                table: "sync_queue_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "entity_type",
                table: "sync_queue_entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "external_part_number",
                table: "parts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "min_stock_threshold",
                table: "parts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preferred_vendor_id",
                table: "parts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "reorder_point",
                table: "parts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tooling_asset_id",
                table: "parts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "disposition",
                table: "jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "disposition_at",
                table: "jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "disposition_notes",
                table: "jobs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "internal_project_type_id",
                table: "jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_internal",
                table: "jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "iteration_count",
                table: "jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "iteration_notes",
                table: "jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "parent_job_id",
                table: "jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "part_id",
                table: "jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "document_type",
                table: "file_attachments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "expiration_date",
                table: "file_attachments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "part_revision_id",
                table: "file_attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "required_role",
                table: "file_attachments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "expenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_ref",
                table: "expenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider",
                table: "expenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lead_time_days",
                table: "bomentries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "reserved_quantity",
                table: "bin_contents",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "cavity_count",
                table: "assets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "current_shot_count",
                table: "assets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_customer_owned",
                table: "assets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "source_job_id",
                table: "assets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source_part_id",
                table: "assets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tool_life_expectancy",
                table: "assets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "accounting_employee_id",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "employee_barcode",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "google_id",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "microsoft_id",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "oidc_provider",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "oidc_subject_id",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pin_hash",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "setup_token",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "setup_token_expires_at",
                table: "asp_net_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "team_id",
                table: "asp_net_users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_log_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    entity_id = table.Column<int>(type: "integer", nullable: true),
                    details = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "barcodes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    part_id = table.Column<int>(type: "integer", nullable: true),
                    job_id = table.Column<int>(type: "integer", nullable: true),
                    sales_order_id = table.Column<int>(type: "integer", nullable: true),
                    purchase_order_id = table.Column<int>(type: "integer", nullable: true),
                    asset_id = table.Column<int>(type: "integer", nullable: true),
                    storage_location_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_barcodes", x => x.id);
                    table.ForeignKey(
                        name: "fk_barcodes__asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_barcodes__jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_barcodes__parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_barcodes__purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_barcodes__sales_orders_sales_order_id",
                        column: x => x.sales_order_id,
                        principalTable: "sales_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_barcodes__storage_locations_storage_location_id",
                        column: x => x.storage_location_id,
                        principalTable: "storage_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_barcodes_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_group = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_rooms", x => x.id);
                    table.ForeignKey(
                        name: "fk_chat_rooms__asp_net_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customer_returns",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    return_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    original_job_id = table.Column<int>(type: "integer", nullable: false),
                    rework_job_id = table.Column<int>(type: "integer", nullable: true),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    inspected_by_id = table.Column<int>(type: "integer", nullable: true),
                    inspected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    inspection_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_returns", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_returns__jobs_original_job_id",
                        column: x => x.original_job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_customer_returns__jobs_rework_job_id",
                        column: x => x.rework_job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_customer_returns_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cycle_counts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    location_id = table.Column<int>(type: "integer", nullable: false),
                    counted_by_id = table.Column<int>(type: "integer", nullable: false),
                    counted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cycle_counts", x => x.id);
                    table.ForeignKey(
                        name: "fk_cycle_counts__asp_net_users_counted_by_id",
                        column: x => x.counted_by_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cycle_counts__storage_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "storage_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_embeddings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    chunk_text = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    chunk_index = table.Column<int>(type: "integer", nullable: false),
                    source_field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    embedding = table.Column<Vector>(type: "vector(384)", nullable: true),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_embeddings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "downtime_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asset_id = table.Column<int>(type: "integer", nullable: false),
                    reported_by_id = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    resolution = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_planned = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_downtime_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_downtime_logs_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_schedules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asset_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    interval_days = table.Column<int>(type: "integer", nullable: false),
                    interval_hours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    last_performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    maintenance_job_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_schedules", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_schedules_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_schedules_jobs_maintenance_job_id",
                        column: x => x.maintenance_job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "part_revisions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    change_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    change_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_part_revisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_part_revisions_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    step_number = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    instructions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    work_center_id = table.Column<int>(type: "integer", nullable: true),
                    estimated_minutes = table.Column<int>(type: "integer", nullable: true),
                    is_qc_checkpoint = table.Column<bool>(type: "boolean", nullable: false),
                    qc_criteria = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_process_steps_assets_work_center_id",
                        column: x => x.work_center_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_process_steps_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "production_runs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_id = table.Column<int>(type: "integer", nullable: false),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    operator_id = table.Column<int>(type: "integer", nullable: true),
                    run_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    target_quantity = table.Column<int>(type: "integer", nullable: false),
                    completed_quantity = table.Column<int>(type: "integer", nullable: false),
                    scrap_quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    setup_time_minutes = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    run_time_minutes = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_production_runs", x => x.id);
                    table.ForeignKey(
                        name: "fk_production_runs_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_production_runs_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "qc_checklist_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    part_id = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qc_checklist_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_qc_checklist_templates_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recurring_expenses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    classification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    vendor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    frequency = table.Column<int>(type: "integer", nullable: false),
                    next_occurrence_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_generated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    auto_approve = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recurring_expenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    bin_content_id = table.Column<int>(type: "integer", nullable: false),
                    job_id = table.Column<int>(type: "integer", nullable: true),
                    sales_order_line_id = table.Column<int>(type: "integer", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_reservations__sales_order_lines_sales_order_line_id",
                        column: x => x.sales_order_line_id,
                        principalTable: "sales_order_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_reservations_bin_contents_bin_content_id",
                        column: x => x.bin_content_id,
                        principalTable: "bin_contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reservations_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_reservations_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_tax_rates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_tax_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "saved_reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    entity_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    columns_json = table.Column<string>(type: "jsonb", nullable: false),
                    filters_json = table.Column<string>(type: "jsonb", nullable: true),
                    group_by_field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    chart_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    chart_label_field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    chart_value_field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_shared = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saved_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_saved_reports__asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    track_type_id = table.Column<int>(type: "integer", nullable: false),
                    internal_project_type_id = table.Column<int>(type: "integer", nullable: true),
                    assignee_id = table.Column<int>(type: "integer", nullable: true),
                    cron_expression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scheduled_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_scheduled_tasks__track_types_track_type_id",
                        column: x => x.track_type_id,
                        principalTable: "track_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_scheduled_tasks_reference_data_internal_project_type_id",
                        column: x => x.internal_project_type_id,
                        principalTable: "reference_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "shipment_packages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shipment_id = table.Column<int>(type: "integer", nullable: false),
                    tracking_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    carrier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    weight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    length = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    width = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    height = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipment_packages", x => x.id);
                    table.ForeignKey(
                        name: "fk_shipment_packages_shipments_shipment_id",
                        column: x => x.shipment_id,
                        principalTable: "shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "status_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    status_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status_label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    set_by_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_status_entries__asp_net_users_set_by_id",
                        column: x => x.set_by_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teams", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_scan_identifiers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    identifier_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    identifier_value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_scan_identifiers", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_scan_identifiers__asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sender_id = table.Column<int>(type: "integer", nullable: false),
                    recipient_id = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    chat_room_id = table.Column<int>(type: "integer", nullable: true),
                    file_attachment_id = table.Column<int>(type: "integer", nullable: true),
                    linked_entity_type = table.Column<string>(type: "text", nullable: true),
                    linked_entity_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_chat_messages__asp_net_users_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chat_messages__asp_net_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chat_messages__chat_rooms_chat_room_id",
                        column: x => x.chat_room_id,
                        principalTable: "chat_rooms",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_chat_messages__file_attachments_file_attachment_id",
                        column: x => x.file_attachment_id,
                        principalTable: "file_attachments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "chat_room_members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    chat_room_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_room_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_chat_room_members__asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chat_room_members_chat_rooms_chat_room_id",
                        column: x => x.chat_room_id,
                        principalTable: "chat_rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cycle_count_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cycle_count_id = table.Column<int>(type: "integer", nullable: false),
                    bin_content_id = table.Column<int>(type: "integer", nullable: true),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    expected_quantity = table.Column<int>(type: "integer", nullable: false),
                    actual_quantity = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cycle_count_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_cycle_count_lines_bin_contents_bin_content_id",
                        column: x => x.bin_content_id,
                        principalTable: "bin_contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_cycle_count_lines_cycle_counts_cycle_count_id",
                        column: x => x.cycle_count_id,
                        principalTable: "cycle_counts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maintenance_schedule_id = table.Column<int>(type: "integer", nullable: false),
                    performed_by_id = table.Column<int>(type: "integer", nullable: false),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hours_at_service = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_logs__maintenance_schedules_maintenance_schedule_~",
                        column: x => x.maintenance_schedule_id,
                        principalTable: "maintenance_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lot_records",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lot_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    job_id = table.Column<int>(type: "integer", nullable: true),
                    production_run_id = table.Column<int>(type: "integer", nullable: true),
                    purchase_order_line_id = table.Column<int>(type: "integer", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    supplier_lot_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lot_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_lot_records__parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lot_records__production_runs_production_run_id",
                        column: x => x.production_run_id,
                        principalTable: "production_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lot_records__purchase_order_lines_purchase_order_line_id",
                        column: x => x.purchase_order_line_id,
                        principalTable: "purchase_order_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lot_records_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "qc_checklist_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    template_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    specification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qc_checklist_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_qc_checklist_items__qc_checklist_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "qc_checklist_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "qc_inspections",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_id = table.Column<int>(type: "integer", nullable: true),
                    production_run_id = table.Column<int>(type: "integer", nullable: true),
                    template_id = table.Column<int>(type: "integer", nullable: true),
                    inspector_id = table.Column<int>(type: "integer", nullable: false),
                    lot_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qc_inspections", x => x.id);
                    table.ForeignKey(
                        name: "fk_qc_inspections_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_qc_inspections_production_runs_production_run_id",
                        column: x => x.production_run_id,
                        principalTable: "production_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_qc_inspections_qc_checklist_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "qc_checklist_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "kiosk_terminals",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    device_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    configured_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_terminals", x => x.id);
                    table.ForeignKey(
                        name: "fk_kiosk_terminals__teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "qc_inspection_results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inspection_id = table.Column<int>(type: "integer", nullable: false),
                    checklist_item_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    passed = table.Column<bool>(type: "boolean", nullable: false),
                    measured_value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qc_inspection_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_qc_inspection_results_qc_checklist_items_checklist_item_id",
                        column: x => x.checklist_item_id,
                        principalTable: "qc_checklist_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_qc_inspection_results_qc_inspections_inspection_id",
                        column: x => x.inspection_id,
                        principalTable: "qc_inspections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sync_queue_entries_entity_type_entity_id",
                table: "sync_queue_entries",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_sync_queue_entries_status_created_at",
                table: "sync_queue_entries",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_parts_preferred_vendor_id",
                table: "parts",
                column: "preferred_vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_parts_tooling_asset_id",
                table: "parts",
                column: "tooling_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_parent_job_id",
                table: "jobs",
                column: "parent_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_part_id",
                table: "jobs",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_file_attachments_part_revision_id",
                table: "file_attachments",
                column: "part_revision_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_source_job_id",
                table: "assets",
                column: "source_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_source_part_id",
                table: "assets",
                column: "source_part_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entries_action",
                table: "audit_log_entries",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entries_created_at",
                table: "audit_log_entries",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entries_entity_type_entity_id",
                table: "audit_log_entries",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entries_user_id",
                table: "audit_log_entries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_barcodes_asset_id",
                table: "barcodes",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_barcodes_entity_type",
                table: "barcodes",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "ix_barcodes_job_id",
                table: "barcodes",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_barcodes_part_id",
                table: "barcodes",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_barcodes_purchase_order_id",
                table: "barcodes",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_barcodes_sales_order_id",
                table: "barcodes",
                column: "sales_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_barcodes_storage_location_id",
                table: "barcodes",
                column: "storage_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_barcodes_user_id",
                table: "barcodes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_barcodes_value",
                table: "barcodes",
                column: "value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_chat_room_id",
                table: "chat_messages",
                column: "chat_room_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_file_attachment_id",
                table: "chat_messages",
                column: "file_attachment_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_recipient_id",
                table: "chat_messages",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_sender_id",
                table: "chat_messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_sender_id_recipient_id_created_at",
                table: "chat_messages",
                columns: new[] { "sender_id", "recipient_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_chat_room_members_chat_room_id",
                table: "chat_room_members",
                column: "chat_room_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_room_members_chat_room_id_user_id",
                table: "chat_room_members",
                columns: new[] { "chat_room_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chat_room_members_user_id",
                table: "chat_room_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_rooms_created_by_id",
                table: "chat_rooms",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_returns_customer_id",
                table: "customer_returns",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_returns_original_job_id",
                table: "customer_returns",
                column: "original_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_returns_return_number",
                table: "customer_returns",
                column: "return_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customer_returns_rework_job_id",
                table: "customer_returns",
                column: "rework_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_returns_status",
                table: "customer_returns",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_cycle_count_lines_bin_content_id",
                table: "cycle_count_lines",
                column: "bin_content_id");

            migrationBuilder.CreateIndex(
                name: "ix_cycle_count_lines_cycle_count_id",
                table: "cycle_count_lines",
                column: "cycle_count_id");

            migrationBuilder.CreateIndex(
                name: "ix_cycle_counts_counted_by_id",
                table: "cycle_counts",
                column: "counted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_cycle_counts_location_id_counted_at",
                table: "cycle_counts",
                columns: new[] { "location_id", "counted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_document_embeddings_entity_type_entity_id",
                table: "document_embeddings",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_downtime_logs_asset_id",
                table: "downtime_logs",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_downtime_logs_started_at",
                table: "downtime_logs",
                column: "started_at");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_terminals_device_token",
                table: "kiosk_terminals",
                column: "device_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_terminals_team_id",
                table: "kiosk_terminals",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_lot_records_job_id",
                table: "lot_records",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_lot_records_lot_number",
                table: "lot_records",
                column: "lot_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lot_records_part_id",
                table: "lot_records",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_lot_records_production_run_id",
                table: "lot_records",
                column: "production_run_id");

            migrationBuilder.CreateIndex(
                name: "ix_lot_records_purchase_order_line_id",
                table: "lot_records",
                column: "purchase_order_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_logs_maintenance_schedule_id",
                table: "maintenance_logs",
                column: "maintenance_schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_asset_id",
                table: "maintenance_schedules",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_maintenance_job_id",
                table: "maintenance_schedules",
                column: "maintenance_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_next_due_at",
                table: "maintenance_schedules",
                column: "next_due_at");

            migrationBuilder.CreateIndex(
                name: "ix_part_revisions_part_id",
                table: "part_revisions",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_part_revisions_part_id_revision",
                table: "part_revisions",
                columns: new[] { "part_id", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_part_id",
                table: "process_steps",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_work_center_id",
                table: "process_steps",
                column: "work_center_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_runs_job_id",
                table: "production_runs",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_runs_part_id",
                table: "production_runs",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_runs_run_number",
                table: "production_runs",
                column: "run_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_production_runs_status",
                table: "production_runs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_qc_checklist_items_template_id",
                table: "qc_checklist_items",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_qc_checklist_templates_part_id",
                table: "qc_checklist_templates",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_qc_inspection_results_checklist_item_id",
                table: "qc_inspection_results",
                column: "checklist_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_qc_inspection_results_inspection_id",
                table: "qc_inspection_results",
                column: "inspection_id");

            migrationBuilder.CreateIndex(
                name: "ix_qc_inspections_inspector_id",
                table: "qc_inspections",
                column: "inspector_id");

            migrationBuilder.CreateIndex(
                name: "ix_qc_inspections_job_id",
                table: "qc_inspections",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_qc_inspections_production_run_id",
                table: "qc_inspections",
                column: "production_run_id");

            migrationBuilder.CreateIndex(
                name: "ix_qc_inspections_template_id",
                table: "qc_inspections",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_expenses_classification",
                table: "recurring_expenses",
                column: "classification");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_expenses_is_active",
                table: "recurring_expenses",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_expenses_next_occurrence_date",
                table: "recurring_expenses",
                column: "next_occurrence_date");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_expenses_user_id",
                table: "recurring_expenses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_bin_content_id",
                table: "reservations",
                column: "bin_content_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_job_id",
                table: "reservations",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_part_id",
                table: "reservations",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_sales_order_line_id",
                table: "reservations",
                column: "sales_order_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_tax_rates_code",
                table: "sales_tax_rates",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_saved_reports_is_shared",
                table: "saved_reports",
                column: "is_shared");

            migrationBuilder.CreateIndex(
                name: "ix_saved_reports_user_id",
                table: "saved_reports",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_tasks_internal_project_type_id",
                table: "scheduled_tasks",
                column: "internal_project_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_tasks_is_active",
                table: "scheduled_tasks",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_tasks_next_run_at",
                table: "scheduled_tasks",
                column: "next_run_at");

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_tasks_track_type_id",
                table: "scheduled_tasks",
                column: "track_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipment_packages_shipment_id",
                table: "shipment_packages",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_status_entries_entity_type_entity_id",
                table: "status_entries",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_status_entries_entity_type_entity_id_category",
                table: "status_entries",
                columns: new[] { "entity_type", "entity_id", "category" });

            migrationBuilder.CreateIndex(
                name: "ix_status_entries_entity_type_entity_id_ended_at",
                table: "status_entries",
                columns: new[] { "entity_type", "entity_id", "ended_at" });

            migrationBuilder.CreateIndex(
                name: "ix_status_entries_set_by_id",
                table: "status_entries",
                column: "set_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_scan_identifiers_identifier_type_identifier_value",
                table: "user_scan_identifiers",
                columns: new[] { "identifier_type", "identifier_value" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_scan_identifiers_user_id",
                table: "user_scan_identifiers",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_assets__jobs_source_job_id",
                table: "assets",
                column: "source_job_id",
                principalTable: "jobs",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_assets__parts_source_part_id",
                table: "assets",
                column: "source_part_id",
                principalTable: "parts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_file_attachments__part_revisions_part_revision_id",
                table: "file_attachments",
                column: "part_revision_id",
                principalTable: "part_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_jobs__parts_part_id",
                table: "jobs",
                column: "part_id",
                principalTable: "parts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_jobs_jobs_parent_job_id",
                table: "jobs",
                column: "parent_job_id",
                principalTable: "jobs",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_parts__vendors_preferred_vendor_id",
                table: "parts",
                column: "preferred_vendor_id",
                principalTable: "vendors",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_parts_assets_tooling_asset_id",
                table: "parts",
                column: "tooling_asset_id",
                principalTable: "assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_assets__jobs_source_job_id",
                table: "assets");

            migrationBuilder.DropForeignKey(
                name: "fk_assets__parts_source_part_id",
                table: "assets");

            migrationBuilder.DropForeignKey(
                name: "fk_file_attachments__part_revisions_part_revision_id",
                table: "file_attachments");

            migrationBuilder.DropForeignKey(
                name: "fk_jobs__parts_part_id",
                table: "jobs");

            migrationBuilder.DropForeignKey(
                name: "fk_jobs_jobs_parent_job_id",
                table: "jobs");

            migrationBuilder.DropForeignKey(
                name: "fk_parts__vendors_preferred_vendor_id",
                table: "parts");

            migrationBuilder.DropForeignKey(
                name: "fk_parts_assets_tooling_asset_id",
                table: "parts");

            migrationBuilder.DropTable(
                name: "audit_log_entries");

            migrationBuilder.DropTable(
                name: "barcodes");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "chat_room_members");

            migrationBuilder.DropTable(
                name: "customer_returns");

            migrationBuilder.DropTable(
                name: "cycle_count_lines");

            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "document_embeddings");

            migrationBuilder.DropTable(
                name: "downtime_logs");

            migrationBuilder.DropTable(
                name: "kiosk_terminals");

            migrationBuilder.DropTable(
                name: "lot_records");

            migrationBuilder.DropTable(
                name: "maintenance_logs");

            migrationBuilder.DropTable(
                name: "part_revisions");

            migrationBuilder.DropTable(
                name: "process_steps");

            migrationBuilder.DropTable(
                name: "qc_inspection_results");

            migrationBuilder.DropTable(
                name: "recurring_expenses");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "sales_tax_rates");

            migrationBuilder.DropTable(
                name: "saved_reports");

            migrationBuilder.DropTable(
                name: "scheduled_tasks");

            migrationBuilder.DropTable(
                name: "shipment_packages");

            migrationBuilder.DropTable(
                name: "status_entries");

            migrationBuilder.DropTable(
                name: "user_scan_identifiers");

            migrationBuilder.DropTable(
                name: "chat_rooms");

            migrationBuilder.DropTable(
                name: "cycle_counts");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "maintenance_schedules");

            migrationBuilder.DropTable(
                name: "qc_checklist_items");

            migrationBuilder.DropTable(
                name: "qc_inspections");

            migrationBuilder.DropTable(
                name: "production_runs");

            migrationBuilder.DropTable(
                name: "qc_checklist_templates");

            migrationBuilder.DropIndex(
                name: "ix_sync_queue_entries_entity_type_entity_id",
                table: "sync_queue_entries");

            migrationBuilder.DropIndex(
                name: "ix_sync_queue_entries_status_created_at",
                table: "sync_queue_entries");

            migrationBuilder.DropIndex(
                name: "ix_parts_preferred_vendor_id",
                table: "parts");

            migrationBuilder.DropIndex(
                name: "ix_parts_tooling_asset_id",
                table: "parts");

            migrationBuilder.DropIndex(
                name: "ix_jobs_parent_job_id",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "ix_jobs_part_id",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "ix_file_attachments_part_revision_id",
                table: "file_attachments");

            migrationBuilder.DropIndex(
                name: "ix_assets_source_job_id",
                table: "assets");

            migrationBuilder.DropIndex(
                name: "ix_assets_source_part_id",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "custom_field_definitions",
                table: "track_types");

            migrationBuilder.DropColumn(
                name: "external_part_number",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "min_stock_threshold",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "preferred_vendor_id",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "reorder_point",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "tooling_asset_id",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "disposition",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "disposition_at",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "disposition_notes",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "internal_project_type_id",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "is_internal",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "iteration_count",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "iteration_notes",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "parent_job_id",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "part_id",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "document_type",
                table: "file_attachments");

            migrationBuilder.DropColumn(
                name: "expiration_date",
                table: "file_attachments");

            migrationBuilder.DropColumn(
                name: "part_revision_id",
                table: "file_attachments");

            migrationBuilder.DropColumn(
                name: "required_role",
                table: "file_attachments");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "external_ref",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "provider",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "lead_time_days",
                table: "bomentries");

            migrationBuilder.DropColumn(
                name: "reserved_quantity",
                table: "bin_contents");

            migrationBuilder.DropColumn(
                name: "cavity_count",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "current_shot_count",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "is_customer_owned",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "source_job_id",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "source_part_id",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "tool_life_expectancy",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "accounting_employee_id",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "employee_barcode",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "google_id",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "microsoft_id",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "oidc_provider",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "oidc_subject_id",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "pin_hash",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "setup_token",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "setup_token_expires_at",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "asp_net_users");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "payload",
                table: "sync_queue_entries",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "operation",
                table: "sync_queue_entries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "entity_type",
                table: "sync_queue_entries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "ix_sync_queue_entries_created_at",
                table: "sync_queue_entries",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_sync_queue_entries_status",
                table: "sync_queue_entries",
                column: "status");
        }
    }
}
