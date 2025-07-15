using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NEXA.Migrations
{
    /// <inheritdoc />
    public partial class Agregarpermisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermisoInstalacion_Proyecto_ProyectoID",
                table: "PermisoInstalacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermisoInstalacion",
                table: "PermisoInstalacion");

            migrationBuilder.RenameTable(
                name: "PermisoInstalacion",
                newName: "PermisosInstalacion");

            migrationBuilder.RenameIndex(
                name: "IX_PermisoInstalacion_ProyectoID",
                table: "PermisosInstalacion",
                newName: "IX_PermisosInstalacion_ProyectoID");

            migrationBuilder.AddColumn<bool>(
                name: "RequiereDocumentos",
                table: "Proyecto",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermisosInstalacion",
                table: "PermisosInstalacion",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PermisosInstalacion_Proyecto_ProyectoID",
                table: "PermisosInstalacion",
                column: "ProyectoID",
                principalTable: "Proyecto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermisosInstalacion_Proyecto_ProyectoID",
                table: "PermisosInstalacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermisosInstalacion",
                table: "PermisosInstalacion");

            migrationBuilder.DropColumn(
                name: "RequiereDocumentos",
                table: "Proyecto");

            migrationBuilder.RenameTable(
                name: "PermisosInstalacion",
                newName: "PermisoInstalacion");

            migrationBuilder.RenameIndex(
                name: "IX_PermisosInstalacion_ProyectoID",
                table: "PermisoInstalacion",
                newName: "IX_PermisoInstalacion_ProyectoID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermisoInstalacion",
                table: "PermisoInstalacion",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PermisoInstalacion_Proyecto_ProyectoID",
                table: "PermisoInstalacion",
                column: "ProyectoID",
                principalTable: "Proyecto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
