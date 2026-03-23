using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "training_modules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    content_type = table.Column<int>(type: "integer", nullable: false),
                    content_json = table.Column<string>(type: "jsonb", nullable: false),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estimated_minutes = table.Column<int>(type: "integer", nullable: false),
                    tags = table.Column<string>(type: "jsonb", nullable: true),
                    app_routes = table.Column<string>(type: "jsonb", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    is_onboarding_required = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_modules", x => x.id);
                    table.ForeignKey(
                        name: "fk_training_modules_asp_net_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "training_paths",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    allowed_roles = table.Column<string>(type: "jsonb", nullable: true),
                    is_auto_assigned = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_paths", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "training_path_modules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    path_id = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_path_modules", x => x.id);
                    table.ForeignKey(
                        name: "fk_training_path_modules_training_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "training_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_training_path_modules_training_paths_path_id",
                        column: x => x.path_id,
                        principalTable: "training_paths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_path_enrollments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    path_id = table.Column<int>(type: "integer", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_auto_assigned = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_path_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "fk_training_path_enrollments_asp_net_users_assigned_by_user_id",
                        column: x => x.assigned_by_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_training_path_enrollments_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_training_path_enrollments_training_paths_path_id",
                        column: x => x.path_id,
                        principalTable: "training_paths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_progress",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    quiz_score = table.Column<int>(type: "integer", nullable: true),
                    quiz_attempts = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    time_spent_seconds = table.Column<int>(type: "integer", nullable: false),
                    quiz_answers_json = table.Column<string>(type: "jsonb", nullable: true),
                    walkthrough_step_reached = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_training_progress", x => x.id);
                    table.ForeignKey(
                        name: "fk_training_progress_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_training_progress_training_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "training_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Indexes for training_modules
            migrationBuilder.CreateIndex(
                name: "ix_training_modules_created_by_user_id",
                table: "training_modules",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_training_modules_is_onboarding_required",
                table: "training_modules",
                column: "is_onboarding_required");

            migrationBuilder.CreateIndex(
                name: "ix_training_modules_is_published",
                table: "training_modules",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "ix_training_modules_slug",
                table: "training_modules",
                column: "slug",
                unique: true);

            // Indexes for training_paths
            migrationBuilder.CreateIndex(
                name: "ix_training_paths_slug",
                table: "training_paths",
                column: "slug",
                unique: true);

            // Indexes for training_path_modules
            migrationBuilder.CreateIndex(
                name: "ix_training_path_modules_module_id",
                table: "training_path_modules",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "ix_training_path_modules_path_id_position",
                table: "training_path_modules",
                columns: new[] { "path_id", "position" });

            // Indexes for training_path_enrollments
            migrationBuilder.CreateIndex(
                name: "ix_training_path_enrollments_assigned_by_user_id",
                table: "training_path_enrollments",
                column: "assigned_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_training_path_enrollments_user_id",
                table: "training_path_enrollments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_training_path_enrollments_user_id_path_id",
                table: "training_path_enrollments",
                columns: new[] { "user_id", "path_id" },
                unique: true);

            // Indexes for training_progress
            migrationBuilder.CreateIndex(
                name: "ix_training_progress_module_id",
                table: "training_progress",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "ix_training_progress_status",
                table: "training_progress",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_training_progress_user_id",
                table: "training_progress",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_training_progress_user_id_module_id",
                table: "training_progress",
                columns: new[] { "user_id", "module_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "training_progress");
            migrationBuilder.DropTable(name: "training_path_enrollments");
            migrationBuilder.DropTable(name: "training_path_modules");
            migrationBuilder.DropTable(name: "training_paths");
            migrationBuilder.DropTable(name: "training_modules");
        }
    }
}
