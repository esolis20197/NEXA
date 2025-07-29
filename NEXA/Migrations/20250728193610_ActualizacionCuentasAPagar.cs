using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NEXA.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionCuentasAPagar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "CuentasPorPagar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GastoId",
                table: "CuentasPorPagar",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_GastoId",
                table: "CuentasPorPagar",
                column: "GastoId");

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasPorPagar_Gastos_GastoId",
                table: "CuentasPorPagar",
                column: "GastoId",
                principalTable: "Gastos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CuentasPorPagar_Gastos_GastoId",
                table: "CuentasPorPagar");

            migrationBuilder.DropIndex(
                name: "IX_CuentasPorPagar_GastoId",
                table: "CuentasPorPagar");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "CuentasPorPagar");

            migrationBuilder.DropColumn(
                name: "GastoId",
                table: "CuentasPorPagar");
        }
    }
}
