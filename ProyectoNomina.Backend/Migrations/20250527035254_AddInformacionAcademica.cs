using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInformacionAcademica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InformacionAcademica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Institucion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaGraduacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoCertificacion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformacionAcademica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InformacionAcademica_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InformacionAcademica_EmpleadoId",
                table: "InformacionAcademica",
                column: "EmpleadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InformacionAcademica");
        }
    }
}
