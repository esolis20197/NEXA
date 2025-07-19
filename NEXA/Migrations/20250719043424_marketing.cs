using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NEXA.Migrations
{
    /// <inheritdoc />
    public partial class marketing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrupoClientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreGrupo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoClientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientesPorGrupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesPorGrupos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesPorGrupos_GrupoClientes_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "GrupoClientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientesPorGrupos_Usuario_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CorreosPromocionales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Asunto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorreosPromocionales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorreosPromocionales_GrupoClientes_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "GrupoClientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorreosPromocionales_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesPorGrupos_ClienteId",
                table: "ClientesPorGrupos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesPorGrupos_GrupoId",
                table: "ClientesPorGrupos",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreosPromocionales_GrupoId",
                table: "CorreosPromocionales",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreosPromocionales_UsuarioId",
                table: "CorreosPromocionales",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientesPorGrupos");

            migrationBuilder.DropTable(
                name: "CorreosPromocionales");

            migrationBuilder.DropTable(
                name: "GrupoClientes");
        }
    }
}
