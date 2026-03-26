using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfFillAndI9Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_training_modules_asp_net_users_created_by_user_id",
                table: "training_modules");

            migrationBuilder.DropForeignKey(
                name: "fk_training_path_enrollments_asp_net_users_assigned_by_user_id",
                table: "training_path_enrollments");

            migrationBuilder.DropForeignKey(
                name: "fk_training_path_enrollments_asp_net_users_user_id",
                table: "training_path_enrollments");

            migrationBuilder.DropForeignKey(
                name: "fk_training_progress_asp_net_users_user_id",
                table: "training_progress");

            migrationBuilder.DropColumn(
                name: "video_generation_error",
                table: "training_modules");

            migrationBuilder.DropColumn(
                name: "video_generation_status",
                table: "training_modules");

            migrationBuilder.DropColumn(
                name: "video_minio_key",
                table: "training_modules");

            migrationBuilder.AlterColumn<string>(
                name: "quiz_session_json",
                table: "training_progress",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "acro_field_map_json",
                table: "compliance_form_templates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "filled_pdf_template_id",
                table: "compliance_form_templates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "filled_pdf_file_id",
                table: "compliance_form_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "i9_document_data_json",
                table: "compliance_form_submissions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "i9_document_list_type",
                table: "compliance_form_submissions",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "i9_employer_user_id",
                table: "compliance_form_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "i9_reverification_due_at",
                table: "compliance_form_submissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "i9_section1_signed_at",
                table: "compliance_form_submissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "i9_section2_overdue_at",
                table: "compliance_form_submissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "i9_section2_signed_at",
                table: "compliance_form_submissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_training_path_enrollments_path_id",
                table: "training_path_enrollments",
                column: "path_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_templates_filled_pdf_template_id",
                table: "compliance_form_templates",
                column: "filled_pdf_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_submissions_filled_pdf_file_id",
                table: "compliance_form_submissions",
                column: "filled_pdf_file_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_submissions_i9_employer_user_id",
                table: "compliance_form_submissions",
                column: "i9_employer_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_submissions_i9_reverification_due_at",
                table: "compliance_form_submissions",
                column: "i9_reverification_due_at");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_form_submissions_i9_section2_overdue_at",
                table: "compliance_form_submissions",
                column: "i9_section2_overdue_at");

            migrationBuilder.AddForeignKey(
                name: "fk_compliance_form_submissions__file_attachments_filled_pdf_file~",
                table: "compliance_form_submissions",
                column: "filled_pdf_file_id",
                principalTable: "file_attachments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_compliance_form_templates__file_attachments_filled_pdf_templa~",
                table: "compliance_form_templates",
                column: "filled_pdf_template_id",
                principalTable: "file_attachments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_training_modules__asp_net_users_created_by_user_id",
                table: "training_modules",
                column: "created_by_user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_training_path_enrollments__asp_net_users_assigned_by_user_id",
                table: "training_path_enrollments",
                column: "assigned_by_user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_training_path_enrollments__asp_net_users_user_id",
                table: "training_path_enrollments",
                column: "user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_training_progress__asp_net_users_user_id",
                table: "training_progress",
                column: "user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_compliance_form_submissions__file_attachments_filled_pdf_file~",
                table: "compliance_form_submissions");

            migrationBuilder.DropForeignKey(
                name: "fk_compliance_form_templates__file_attachments_filled_pdf_templa~",
                table: "compliance_form_templates");

            migrationBuilder.DropForeignKey(
                name: "fk_training_modules__asp_net_users_created_by_user_id",
                table: "training_modules");

            migrationBuilder.DropForeignKey(
                name: "fk_training_path_enrollments__asp_net_users_assigned_by_user_id",
                table: "training_path_enrollments");

            migrationBuilder.DropForeignKey(
                name: "fk_training_path_enrollments__asp_net_users_user_id",
                table: "training_path_enrollments");

            migrationBuilder.DropForeignKey(
                name: "fk_training_progress__asp_net_users_user_id",
                table: "training_progress");

            migrationBuilder.DropIndex(
                name: "ix_training_path_enrollments_path_id",
                table: "training_path_enrollments");

            migrationBuilder.DropIndex(
                name: "ix_compliance_form_templates_filled_pdf_template_id",
                table: "compliance_form_templates");

            migrationBuilder.DropIndex(
                name: "ix_compliance_form_submissions_filled_pdf_file_id",
                table: "compliance_form_submissions");

            migrationBuilder.DropIndex(
                name: "ix_compliance_form_submissions_i9_employer_user_id",
                table: "compliance_form_submissions");

            migrationBuilder.DropIndex(
                name: "ix_compliance_form_submissions_i9_reverification_due_at",
                table: "compliance_form_submissions");

            migrationBuilder.DropIndex(
                name: "ix_compliance_form_submissions_i9_section2_overdue_at",
                table: "compliance_form_submissions");

            migrationBuilder.DropColumn(
                name: "acro_field_map_json",
                table: "compliance_form_templates");

            migrationBuilder.DropColumn(
                name: "filled_pdf_template_id",
                table: "compliance_form_templates");

            migrationBuilder.DropColumn(
                name: "filled_pdf_file_id",
                table: "compliance_form_submissions");

            migrationBuilder.DropColumn(
                name: "i9_document_data_json",
                table: "compliance_form_submissions");

            migrationBuilder.DropColumn(
                name: "i9_document_list_type",
                table: "compliance_form_submissions");

            migrationBuilder.DropColumn(
                name: "i9_employer_user_id",
                table: "compliance_form_submissions");

            migrationBuilder.DropColumn(
                name: "i9_reverification_due_at",
                table: "compliance_form_submissions");

            migrationBuilder.DropColumn(
                name: "i9_section1_signed_at",
                table: "compliance_form_submissions");

            migrationBuilder.DropColumn(
                name: "i9_section2_overdue_at",
                table: "compliance_form_submissions");

            migrationBuilder.DropColumn(
                name: "i9_section2_signed_at",
                table: "compliance_form_submissions");

            migrationBuilder.AlterColumn<string>(
                name: "quiz_session_json",
                table: "training_progress",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_generation_error",
                table: "training_modules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "video_generation_status",
                table: "training_modules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "video_minio_key",
                table: "training_modules",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_training_modules_asp_net_users_created_by_user_id",
                table: "training_modules",
                column: "created_by_user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_training_path_enrollments_asp_net_users_assigned_by_user_id",
                table: "training_path_enrollments",
                column: "assigned_by_user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_training_path_enrollments_asp_net_users_user_id",
                table: "training_path_enrollments",
                column: "user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_training_progress_asp_net_users_user_id",
                table: "training_progress",
                column: "user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
