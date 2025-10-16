using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHorasYTarifasToDetalleNomina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtra",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasRegulares",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TarifaExtra",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TarifaHora",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorasExtra",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "HorasRegulares",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "TarifaExtra",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "TarifaHora",
                table: "DetalleNominas");
        }
    }
}
