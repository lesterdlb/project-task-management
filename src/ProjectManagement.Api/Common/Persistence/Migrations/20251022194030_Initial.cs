using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Api.Common.Persistence.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: false),
                last_modified_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "Projects",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: false),
                last_modified_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_projects", x => x.id);
                table.ForeignKey(
                    name: "fk_projects_users_owner_id",
                    column: x => x.owner_id,
                    principalTable: "Users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Labels",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: false),
                last_modified_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_labels", x => x.id);
                table.ForeignKey(
                    name: "fk_labels_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "Projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProjectMembers",
            columns: table => new
            {
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_in_project = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                date_joined = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_project_members", x => new { x.project_id, x.user_id });
                table.ForeignKey(
                    name: "fk_project_members_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "Projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_project_members_users_user_id",
                    column: x => x.user_id,
                    principalTable: "Users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Tasks",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: false),
                last_modified_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tasks", x => x.id);
                table.ForeignKey(
                    name: "fk_tasks_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "Projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_tasks_users_assigned_to_id",
                    column: x => x.assigned_to_id,
                    principalTable: "Users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_tasks_users_created_by",
                    column: x => x.created_by,
                    principalTable: "Users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Comments",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                author_id = table.Column<Guid>(type: "uuid", nullable: false),
                project_task_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: false),
                last_modified_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_comments", x => x.id);
                table.ForeignKey(
                    name: "fk_comments_project_tasks_project_task_id",
                    column: x => x.project_task_id,
                    principalTable: "Tasks",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_comments_users_author_id",
                    column: x => x.author_id,
                    principalTable: "Users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_comments_author_id",
            table: "Comments",
            column: "author_id");

        migrationBuilder.CreateIndex(
            name: "ix_comments_project_task_id",
            table: "Comments",
            column: "project_task_id");

        migrationBuilder.CreateIndex(
            name: "ix_labels_project_id",
            table: "Labels",
            column: "project_id");

        migrationBuilder.CreateIndex(
            name: "ix_project_members_user_id",
            table: "ProjectMembers",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_projects_name",
            table: "Projects",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_projects_owner_id",
            table: "Projects",
            column: "owner_id");

        migrationBuilder.CreateIndex(
            name: "ix_tasks_assigned_to_id",
            table: "Tasks",
            column: "assigned_to_id");

        migrationBuilder.CreateIndex(
            name: "ix_tasks_created_by",
            table: "Tasks",
            column: "created_by");

        migrationBuilder.CreateIndex(
            name: "ix_tasks_project_id",
            table: "Tasks",
            column: "project_id");

        migrationBuilder.CreateIndex(
            name: "ix_tasks_title",
            table: "Tasks",
            column: "title",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            table: "Users",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_user_name",
            table: "Users",
            column: "user_name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Comments");

        migrationBuilder.DropTable(
            name: "Labels");

        migrationBuilder.DropTable(
            name: "ProjectMembers");

        migrationBuilder.DropTable(
            name: "Tasks");

        migrationBuilder.DropTable(
            name: "Projects");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
