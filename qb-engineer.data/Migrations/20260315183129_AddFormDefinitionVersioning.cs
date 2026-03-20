using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFormDefinitionVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sensitivity",
                table: "file_attachments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "work_location_id",
                table: "asp_net_users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ai_assistants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    system_prompt = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false),
                    allowed_entity_types = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    starter_questions = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_built_in = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    temperature = table.Column<double>(type: "double precision", nullable: false),
                    max_context_chunks = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_assistants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compliance_form_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    form_type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sha256_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    is_auto_sync = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    requires_identity_docs = table.Column<bool>(type: "boolean", nullable: false),
                    docu_seal_template_id = table.Column<int>(type: "integer", nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    manual_override_file_id = table.Column<int>(type: "integer", nullable: true),
                    blocks_job_assignment = table.Column<bool>(type: "boolean", nullable: false),
                    profile_completion_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compliance_form_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_compliance_form_templates__file_attachments_manual_override_f~",
                        column: x => x.manual_override_file_id,
                        principalTable: "file_attachments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "identity_documents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    document_type = table.Column<int>(type: "integer", nullable: false),
                    file_attachment_id = table.Column<int>(type: "integer", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_by_id = table.Column<int>(type: "integer", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_documents_file_attachments_file_attachment_id",
                        column: x => x.file_attachment_id,
                        principalTable: "file_attachments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pay_stubs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    pay_period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pay_period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pay_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gross_pay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    net_pay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_deductions = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_taxes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    file_attachment_id = table.Column<int>(type: "integer", nullable: true),
                    source = table.Column<int>(type: "integer", nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pay_stubs", x => x.id);
                    table.ForeignKey(
                        name: "fk_pay_stubs_file_attachments_file_attachment_id",
                        column: x => x.file_attachment_id,
                        principalTable: "file_attachments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tax_documents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    document_type = table.Column<int>(type: "integer", nullable: false),
                    tax_year = table.Column<int>(type: "integer", nullable: false),
                    employer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    file_attachment_id = table.Column<int>(type: "integer", nullable: true),
                    source = table.Column<int>(type: "integer", nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_tax_documents_file_attachments_file_attachment_id",
                        column: x => x.file_attachment_id,
                        principalTable: "file_attachments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "form_definition_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    template_id = table.Column<int>(type: "integer", nullable: true),
                    state_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    form_definition_json = table.Column<string>(type: "jsonb", nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sha256_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revision = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    extracted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    field_count = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_definition_versions", x => x.id);
                    table.CheckConstraint("ck_form_definition_versions_scope", "template_id IS NOT NULL OR state_code IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_form_definition_versions_compliance_form_templates_template~",
                        column: x => x.template_id,
                        principalTable: "compliance_form_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pay_stub_deductions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pay_stub_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pay_stub_deductions", x => x.id);
                    table.ForeignKey(
                        name: "fk_pay_stub_deductions_pay_stubs_pay_stub_id",
                        column: x => x.pay_stub_id,
                        principalTable: "pay_stubs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliance_form_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    template_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    docu_seal_submission_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    signed_pdf_file_id = table.Column<int>(type: "integer", nullable: true),
                    docu_seal_submit_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    form_data_json = table.Column<string>(type: "jsonb", nullable: true),
                    form_definition_version_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compliance_form_submissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_compliance_form_submissions__compliance_form_templates_templat~",
                        column: x => x.template_id,
                        principalTable: "compliance_form_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_compliance_form_submissions__file_attachments_signed_pdf_file~",
                        column: x => x.signed_pdf_file_id,
                        principalTable: "file_attachments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_compliance_form_submissions__form_definition_versions_form_def~",
                        column: x => x.form_definition_version_id,
                        principalTable: "form_definition_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_work_location_id",
                table: "asp_net_users",
                column: "work_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_assistants_is_active_sort_order",
                table: "ai_assistants",
                columns: new[] { "is_active", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_company_locations_is_default",
                table: "company_locations",
                column: "is_default",
                unique: true,
                filter: "is_default = true");

            migrationBuilder.CreateIndex(
                name: "ix_company_locations_state",
                table: "company_locations",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_submissions_form_definition_version_id",
                table: "compliance_form_submissions",
                column: "form_definition_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_submissions_signed_pdf_file_id",
                table: "compliance_form_submissions",
                column: "signed_pdf_file_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_submissions_template_id",
                table: "compliance_form_submissions",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_submissions_user_id",
                table: "compliance_form_submissions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_submissions_user_id_template_id",
                table: "compliance_form_submissions",
                columns: new[] { "user_id", "template_id" });

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_templates_form_type",
                table: "compliance_form_templates",
                column: "form_type");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_templates_manual_override_file_id",
                table: "compliance_form_templates",
                column: "manual_override_file_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_definition_versions_state_code_effective_date",
                table: "form_definition_versions",
                columns: new[] { "state_code", "effective_date" });

            migrationBuilder.CreateIndex(
                name: "ix_form_definition_versions_template_id_effective_date",
                table: "form_definition_versions",
                columns: new[] { "template_id", "effective_date" });

            migrationBuilder.CreateIndex(
                name: "ix_identity_documents_file_attachment_id",
                table: "identity_documents",
                column: "file_attachment_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_documents_user_id",
                table: "identity_documents",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_documents_verified_by_id",
                table: "identity_documents",
                column: "verified_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_pay_stub_deductions_pay_stub_id",
                table: "pay_stub_deductions",
                column: "pay_stub_id");

            migrationBuilder.CreateIndex(
                name: "ix_pay_stubs_external_id",
                table: "pay_stubs",
                column: "external_id",
                unique: true,
                filter: "external_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_pay_stubs_file_attachment_id",
                table: "pay_stubs",
                column: "file_attachment_id");

            migrationBuilder.CreateIndex(
                name: "ix_pay_stubs_user_id",
                table: "pay_stubs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tax_documents_external_id",
                table: "tax_documents",
                column: "external_id",
                unique: true,
                filter: "external_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tax_documents_file_attachment_id",
                table: "tax_documents",
                column: "file_attachment_id");

            migrationBuilder.CreateIndex(
                name: "ix_tax_documents_user_id_tax_year",
                table: "tax_documents",
                columns: new[] { "user_id", "tax_year" });

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_users_company_locations_work_location_id",
                table: "asp_net_users",
                column: "work_location_id",
                principalTable: "company_locations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_users_company_locations_work_location_id",
                table: "asp_net_users");

            migrationBuilder.DropTable(
                name: "ai_assistants");

            migrationBuilder.DropTable(
                name: "company_locations");

            migrationBuilder.DropTable(
                name: "compliance_form_submissions");

            migrationBuilder.DropTable(
                name: "identity_documents");

            migrationBuilder.DropTable(
                name: "pay_stub_deductions");

            migrationBuilder.DropTable(
                name: "tax_documents");

            migrationBuilder.DropTable(
                name: "form_definition_versions");

            migrationBuilder.DropTable(
                name: "pay_stubs");

            migrationBuilder.DropTable(
                name: "compliance_form_templates");

            migrationBuilder.DropIndex(
                name: "ix_asp_net_users_work_location_id",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "sensitivity",
                table: "file_attachments");

            migrationBuilder.DropColumn(
                name: "work_location_id",
                table: "asp_net_users");
        }
    }
}
