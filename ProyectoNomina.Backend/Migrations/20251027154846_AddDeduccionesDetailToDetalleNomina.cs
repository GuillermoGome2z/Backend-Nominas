using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDeduccionesDetailToDetalleNomina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Anticipos",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtrasDeducciones",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Prestamos",
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
                name: "Anticipos",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "OtrasDeducciones",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "Prestamos",
                table: "DetalleNominas");
        }
    }
}
