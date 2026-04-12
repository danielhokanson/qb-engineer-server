using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNcrCapaEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CorrectiveAction first (referenced by NonConformance.CapaId and CapaTask.CapaId)
            migrationBuilder.CreateTable(
                name: "corrective_actions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    capa_number = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    source_entity_id = table.Column<int>(type: "integer", nullable: true),
                    source_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    problem_description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    impact_description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    root_cause_analysis = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    root_cause_method = table.Column<int>(type: "integer", nullable: true),
                    root_cause_method_data = table.Column<string>(type: "jsonb", nullable: true),
                    root_cause_analyzed_by_id = table.Column<int>(type: "integer", nullable: true),
                    root_cause_completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    containment_action = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    corrective_action_description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    preventive_action = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    verification_method = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    verification_result = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    verified_by_id = table.Column<int>(type: "integer", nullable: true),
                    verification_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    effectiveness_check_due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    effectiveness_check_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    effectiveness_result = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    is_effective = table.Column<bool>(type: "boolean", nullable: true),
                    effectiveness_checked_by_id = table.Column<int>(type: "integer", nullable: true),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_by_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_corrective_actions", x => x.id);
                    table.ForeignKey(
                        name: "fk_corrective_actions_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_corrective_actions_users_closed_by_id",
                        column: x => x.closed_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_corrective_actions_users_root_cause_analyzed_by_id",
                        column: x => x.root_cause_analyzed_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_corrective_actions_users_verified_by_id",
                        column: x => x.verified_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_corrective_actions_users_effectiveness_checked_by_id",
                        column: x => x.effectiveness_checked_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // NonConformance (references corrective_actions)
            migrationBuilder.CreateTable(
                name: "non_conformances",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ncr_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    job_id = table.Column<int>(type: "integer", nullable: true),
                    production_run_id = table.Column<int>(type: "integer", nullable: true),
                    lot_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sales_order_line_id = table.Column<int>(type: "integer", nullable: true),
                    purchase_order_line_id = table.Column<int>(type: "integer", nullable: true),
                    qc_inspection_id = table.Column<int>(type: "integer", nullable: true),
                    detected_by_id = table.Column<int>(type: "integer", nullable: false),
                    detected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    detected_at_stage = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    affected_quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    defective_quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    containment_actions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    containment_by_id = table.Column<int>(type: "integer", nullable: true),
                    containment_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disposition_code = table.Column<int>(type: "integer", nullable: true),
                    disposition_by_id = table.Column<int>(type: "integer", nullable: true),
                    disposition_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disposition_notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    rework_instructions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    material_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    labor_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    total_cost_impact = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    capa_id = table.Column<int>(type: "integer", nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    vendor_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_non_conformances", x => x.id);
                    table.ForeignKey(
                        name: "fk_non_conformances_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_non_conformances_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_non_conformances_corrective_actions_capa_id",
                        column: x => x.capa_id,
                        principalTable: "corrective_actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_non_conformances_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_non_conformances_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_non_conformances_users_detected_by_id",
                        column: x => x.detected_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_non_conformances_users_containment_by_id",
                        column: x => x.containment_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_non_conformances_users_disposition_by_id",
                        column: x => x.disposition_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // CapaTask (references corrective_actions)
            migrationBuilder.CreateTable(
                name: "capa_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    capa_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    assignee_id = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_by_id = table.Column<int>(type: "integer", nullable: true),
                    completion_notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_capa_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_capa_tasks_corrective_actions_capa_id",
                        column: x => x.capa_id,
                        principalTable: "corrective_actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_capa_tasks_users_assignee_id",
                        column: x => x.assignee_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_capa_tasks_users_completed_by_id",
                        column: x => x.completed_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Indexes — corrective_actions
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_capa_number", table: "corrective_actions", column: "capa_number", unique: true);
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_owner_id", table: "corrective_actions", column: "owner_id");
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_status", table: "corrective_actions", column: "status");
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_priority", table: "corrective_actions", column: "priority");
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_due_date", table: "corrective_actions", column: "due_date");
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_closed_by_id", table: "corrective_actions", column: "closed_by_id");
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_root_cause_analyzed_by_id", table: "corrective_actions", column: "root_cause_analyzed_by_id");
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_verified_by_id", table: "corrective_actions", column: "verified_by_id");
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_effectiveness_checked_by_id", table: "corrective_actions", column: "effectiveness_checked_by_id");
            migrationBuilder.CreateIndex(name: "ix_corrective_actions_source_entity_id", table: "corrective_actions", column: "source_entity_id");

            // Indexes — non_conformances
            migrationBuilder.CreateIndex(name: "ix_non_conformances_ncr_number", table: "non_conformances", column: "ncr_number", unique: true);
            migrationBuilder.CreateIndex(name: "ix_non_conformances_part_id", table: "non_conformances", column: "part_id");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_job_id", table: "non_conformances", column: "job_id");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_capa_id", table: "non_conformances", column: "capa_id");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_customer_id", table: "non_conformances", column: "customer_id");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_vendor_id", table: "non_conformances", column: "vendor_id");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_detected_by_id", table: "non_conformances", column: "detected_by_id");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_status", table: "non_conformances", column: "status");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_type", table: "non_conformances", column: "type");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_qc_inspection_id", table: "non_conformances", column: "qc_inspection_id");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_production_run_id", table: "non_conformances", column: "production_run_id");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_sales_order_line_id", table: "non_conformances", column: "sales_order_line_id");
            migrationBuilder.CreateIndex(name: "ix_non_conformances_purchase_order_line_id", table: "non_conformances", column: "purchase_order_line_id");

            // Indexes — capa_tasks
            migrationBuilder.CreateIndex(name: "ix_capa_tasks_capa_id", table: "capa_tasks", column: "capa_id");
            migrationBuilder.CreateIndex(name: "ix_capa_tasks_assignee_id", table: "capa_tasks", column: "assignee_id");
            migrationBuilder.CreateIndex(name: "ix_capa_tasks_status", table: "capa_tasks", column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "capa_tasks");
            migrationBuilder.DropTable(name: "non_conformances");
            migrationBuilder.DropTable(name: "corrective_actions");
        }
    }
}
