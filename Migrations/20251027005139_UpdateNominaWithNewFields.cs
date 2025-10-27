using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNominaWithNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Periodo",
                table: "Nominas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Mes",
                table: "Nominas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Nominas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Anio",
                table: "Nominas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AprobadoPor",
                table: "Nominas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CantidadEmpleados",
                table: "Nominas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Nominas",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoNomina",
                table: "Nominas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_Anio_Mes",
                table: "Nominas",
                columns: new[] { "Anio", "Mes" });

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_Estado",
                table: "Nominas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_Periodo_TipoNomina",
                table: "Nominas",
                columns: new[] { "Periodo", "TipoNomina" },
                unique: true,
                filter: "[Periodo] IS NOT NULL AND [Estado] <> 'ANULADA'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Nominas_Anio_Mes",
                table: "Nominas");

            migrationBuilder.DropIndex(
                name: "IX_Nominas_Estado",
                table: "Nominas");

            migrationBuilder.DropIndex(
                name: "IX_Nominas_Periodo_TipoNomina",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "AprobadoPor",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "CantidadEmpleados",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "TipoNomina",
                table: "Nominas");

            migrationBuilder.AlterColumn<string>(
                name: "Periodo",
                table: "Nominas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Mes",
                table: "Nominas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Nominas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "Anio",
                table: "Nominas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
