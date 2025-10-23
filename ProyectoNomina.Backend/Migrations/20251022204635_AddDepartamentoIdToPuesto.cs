using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartamentoIdToPuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartamentoId",
                table: "Puestos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Puestos_DepartamentoId",
                table: "Puestos",
                column: "DepartamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Puestos_Departamentos_DepartamentoId",
                table: "Puestos",
                column: "DepartamentoId",
                principalTable: "Departamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Puestos_Departamentos_DepartamentoId",
                table: "Puestos");

            migrationBuilder.DropIndex(
                name: "IX_Puestos_DepartamentoId",
                table: "Puestos");

            migrationBuilder.DropColumn(
                name: "DepartamentoId",
                table: "Puestos");
        }
    }
}
