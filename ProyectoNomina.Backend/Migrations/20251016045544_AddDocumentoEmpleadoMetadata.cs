using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentoEmpleadoMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "DocumentosEmpleado",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "DocumentosEmpleado",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "DocumentosEmpleado",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreOriginal",
                table: "DocumentosEmpleado",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "DocumentosEmpleado",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubidoPorUsuarioId",
                table: "DocumentosEmpleado",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Tamano",
                table: "DocumentosEmpleado",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "DocumentosEmpleado");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "DocumentosEmpleado");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "DocumentosEmpleado");

            migrationBuilder.DropColumn(
                name: "NombreOriginal",
                table: "DocumentosEmpleado");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "DocumentosEmpleado");

            migrationBuilder.DropColumn(
                name: "SubidoPorUsuarioId",
                table: "DocumentosEmpleado");

            migrationBuilder.DropColumn(
                name: "Tamano",
                table: "DocumentosEmpleado");
        }
    }
}
