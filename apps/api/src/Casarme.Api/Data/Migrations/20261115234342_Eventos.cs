using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Casarme.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Eventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "app",
                table: "Events",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                schema: "app",
                table: "Events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasReception",
                schema: "app",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "app",
                table: "Events",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RsvpDeadline",
                schema: "app",
                table: "Events",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                schema: "app",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "HasReception",
                schema: "app",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "app",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RsvpDeadline",
                schema: "app",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "app",
                table: "Events",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }
    }
}
