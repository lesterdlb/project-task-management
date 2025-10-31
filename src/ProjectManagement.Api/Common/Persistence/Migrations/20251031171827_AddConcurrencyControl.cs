using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Api.Common.Persistence.Migrations;

/// <inheritdoc />
public partial class AddConcurrencyControl : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            table: "Users",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            table: "Tasks",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            table: "Projects",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            table: "Labels",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            table: "Comments",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "xmin",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "xmin",
            table: "Tasks");

        migrationBuilder.DropColumn(
            name: "xmin",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "xmin",
            table: "Labels");

        migrationBuilder.DropColumn(
            name: "xmin",
            table: "Comments");
    }
}
