using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRelacionesDeduccionesBonificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpleadoId",
                table: "Deducciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpleadoId",
                table: "Bonificaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Deducciones_EmpleadoId",
                table: "Deducciones",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Bonificaciones_EmpleadoId",
                table: "Bonificaciones",
                column: "EmpleadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bonificaciones_Empleados_EmpleadoId",
                table: "Bonificaciones",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deducciones_Empleados_EmpleadoId",
                table: "Deducciones",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bonificaciones_Empleados_EmpleadoId",
                table: "Bonificaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Deducciones_Empleados_EmpleadoId",
                table: "Deducciones");

            migrationBuilder.DropIndex(
                name: "IX_Deducciones_EmpleadoId",
                table: "Deducciones");

            migrationBuilder.DropIndex(
                name: "IX_Bonificaciones_EmpleadoId",
                table: "Bonificaciones");

            migrationBuilder.DropColumn(
                name: "EmpleadoId",
                table: "Deducciones");

            migrationBuilder.DropColumn(
                name: "EmpleadoId",
                table: "Bonificaciones");
        }
    }
}
