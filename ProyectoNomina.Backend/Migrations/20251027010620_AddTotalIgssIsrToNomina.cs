using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalIgssIsrToNomina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalIgssEmpleado",
                table: "Nominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalIsr",
                table: "Nominas",
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
                name: "TotalIgssEmpleado",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "TotalIsr",
                table: "Nominas");
        }
    }
}
