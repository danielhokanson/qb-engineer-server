using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpcEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "spc_characteristics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    part_id = table.Column<int>(type: "integer", nullable: false),
                    operation_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    measurement_type = table.Column<int>(type: "integer", nullable: false),
                    nominal_value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    upper_spec_limit = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    lower_spec_limit = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    unit_of_measure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    decimal_places = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    sample_size = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    sample_frequency = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    gage_id = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notify_on_ooc = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spc_characteristics", x => x.id);
                    table.ForeignKey(
                        name: "fk_spc_characteristics_parts_part_id",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_spc_characteristics_operations_operation_id",
                        column: x => x.operation_id,
                        principalTable: "operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "spc_measurements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    characteristic_id = table.Column<int>(type: "integer", nullable: false),
                    job_id = table.Column<int>(type: "integer", nullable: true),
                    production_run_id = table.Column<int>(type: "integer", nullable: true),
                    lot_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    measured_by_id = table.Column<int>(type: "integer", nullable: false),
                    measured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    subgroup_number = table.Column<int>(type: "integer", nullable: false),
                    values_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    mean = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    range = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    std_dev = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    median = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    is_out_of_spec = table.Column<bool>(type: "boolean", nullable: false),
                    is_out_of_control = table.Column<bool>(type: "boolean", nullable: false),
                    ooc_rule_violated = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spc_measurements", x => x.id);
                    table.ForeignKey(
                        name: "fk_spc_measurements_spc_characteristics_characteristic_id",
                        column: x => x.characteristic_id,
                        principalTable: "spc_characteristics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_spc_measurements_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_spc_measurements_asp_net_users_measured_by_id",
                        column: x => x.measured_by_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "spc_control_limits",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    characteristic_id = table.Column<int>(type: "integer", nullable: false),
                    calculated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sample_count = table.Column<int>(type: "integer", nullable: false),
                    from_subgroup = table.Column<int>(type: "integer", nullable: false),
                    to_subgroup = table.Column<int>(type: "integer", nullable: false),
                    x_bar_ucl = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    x_bar_lcl = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    x_bar_center_line = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    range_ucl = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    range_lcl = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    range_center_line = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    s_ucl = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    s_lcl = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    s_center_line = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    cp = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    cpk = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    pp = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    ppk = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    process_sigma = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spc_control_limits", x => x.id);
                    table.ForeignKey(
                        name: "fk_spc_control_limits_spc_characteristics_characteristic_id",
                        column: x => x.characteristic_id,
                        principalTable: "spc_characteristics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spc_ooc_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    characteristic_id = table.Column<int>(type: "integer", nullable: false),
                    measurement_id = table.Column<int>(type: "integer", nullable: false),
                    detected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    rule_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    severity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    acknowledged_by_id = table.Column<int>(type: "integer", nullable: true),
                    acknowledged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    acknowledgment_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    capa_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spc_ooc_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_spc_ooc_events_spc_characteristics_characteristic_id",
                        column: x => x.characteristic_id,
                        principalTable: "spc_characteristics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_spc_ooc_events_spc_measurements_measurement_id",
                        column: x => x.measurement_id,
                        principalTable: "spc_measurements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_spc_ooc_events_asp_net_users_acknowledged_by_id",
                        column: x => x.acknowledged_by_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Indexes for spc_characteristics
            migrationBuilder.CreateIndex(
                name: "ix_spc_characteristics_part_id",
                table: "spc_characteristics",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_characteristics_operation_id",
                table: "spc_characteristics",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_characteristics_gage_id",
                table: "spc_characteristics",
                column: "gage_id");

            // Indexes for spc_measurements
            migrationBuilder.CreateIndex(
                name: "ix_spc_measurements_characteristic_id",
                table: "spc_measurements",
                column: "characteristic_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_measurements_job_id",
                table: "spc_measurements",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_measurements_production_run_id",
                table: "spc_measurements",
                column: "production_run_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_measurements_measured_by_id",
                table: "spc_measurements",
                column: "measured_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_measurements_characteristic_id_subgroup_number",
                table: "spc_measurements",
                columns: new[] { "characteristic_id", "subgroup_number" });

            // Indexes for spc_control_limits
            migrationBuilder.CreateIndex(
                name: "ix_spc_control_limits_characteristic_id",
                table: "spc_control_limits",
                column: "characteristic_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_control_limits_characteristic_id_is_active",
                table: "spc_control_limits",
                columns: new[] { "characteristic_id", "is_active" });

            // Indexes for spc_ooc_events
            migrationBuilder.CreateIndex(
                name: "ix_spc_ooc_events_characteristic_id",
                table: "spc_ooc_events",
                column: "characteristic_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_ooc_events_measurement_id",
                table: "spc_ooc_events",
                column: "measurement_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_ooc_events_acknowledged_by_id",
                table: "spc_ooc_events",
                column: "acknowledged_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_ooc_events_capa_id",
                table: "spc_ooc_events",
                column: "capa_id");

            migrationBuilder.CreateIndex(
                name: "ix_spc_ooc_events_status",
                table: "spc_ooc_events",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "spc_ooc_events");
            migrationBuilder.DropTable(name: "spc_control_limits");
            migrationBuilder.DropTable(name: "spc_measurements");
            migrationBuilder.DropTable(name: "spc_characteristics");
        }
    }
}
