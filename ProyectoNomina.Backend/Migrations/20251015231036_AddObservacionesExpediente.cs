using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddObservacionesExpediente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObservacionesExpediente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    DocumentoEmpleadoId = table.Column<int>(type: "int", nullable: true),
                    Texto = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObservacionesExpediente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObservacionesExpediente_DocumentosEmpleado_DocumentoEmpleadoId",
                        column: x => x.DocumentoEmpleadoId,
                        principalTable: "DocumentosEmpleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ObservacionesExpediente_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObservacionesExpediente_DocumentoEmpleadoId",
                table: "ObservacionesExpediente",
                column: "DocumentoEmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ObservacionesExpediente_EmpleadoId_DocumentoEmpleadoId",
                table: "ObservacionesExpediente",
                columns: new[] { "EmpleadoId", "DocumentoEmpleadoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ObservacionesExpediente_FechaCreacion",
                table: "ObservacionesExpediente",
                column: "FechaCreacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObservacionesExpediente");
        }
    }
}
