using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Casarme.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CadastroCasamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.CreateTable(
                name: "Casamentos",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoupleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Identifier = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Casamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CasamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsPrincipal = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Casamentos_CasamentoId",
                        column: x => x.CasamentoId,
                        principalSchema: "app",
                        principalTable: "Casamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Noivos",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<string>(type: "text", nullable: false),
                    CasamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Noivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Noivos_Casamentos_CasamentoId",
                        column: x => x.CasamentoId,
                        principalSchema: "app",
                        principalTable: "Casamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Casamentos_Identifier",
                schema: "app",
                table: "Casamentos",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_CasamentoId",
                schema: "app",
                table: "Events",
                column: "CasamentoId",
                unique: true,
                filter: "\"IsPrincipal\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Noivos_AccountId",
                schema: "app",
                table: "Noivos",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Noivos_CasamentoId",
                schema: "app",
                table: "Noivos",
                column: "CasamentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Noivos",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Casamentos",
                schema: "app");
        }
    }
}
