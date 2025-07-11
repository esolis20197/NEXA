using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NEXA.Migrations
{
    /// <inheritdoc />
    public partial class migracionCitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCita = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoCita = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Encargado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCita = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Detalles = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroTelefonico = table.Column<int>(type: "int", nullable: false),
                    ProducConsulta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Citas_Usuario_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_UsuarioID_FechaCita",
                table: "Citas",
                columns: new[] { "UsuarioID", "FechaCita" },
                unique: true,
                filter: "[UsuarioID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Citas");
        }
    }
}
