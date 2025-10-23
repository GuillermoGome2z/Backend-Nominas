using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RefactorizacionModelos2025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Puestos_PuestoId",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Puestos_DepartamentoId",
                table: "Puestos");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_Correo",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_DPI",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_NIT",
                table: "Empleados");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Puestos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Puestos",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Empleados",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "PuestoId",
                table: "Empleados",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "NombreCompleto",
                table: "Empleados",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NIT",
                table: "Empleados",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Empleados",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "EstadoLaboral",
                table: "Empleados",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVO",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Empleados",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DPI",
                table: "Empleados",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Empleados",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Departamentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Departamentos",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_Puestos_DepartamentoId_Activo",
                table: "Puestos",
                columns: new[] { "DepartamentoId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Correo",
                table: "Empleados",
                column: "Correo",
                unique: true,
                filter: "[Correo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_DPI",
                table: "Empleados",
                column: "DPI",
                unique: true,
                filter: "[DPI] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_NIT",
                table: "Empleados",
                column: "NIT",
                unique: true,
                filter: "[NIT] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Departamentos_Activo",
                table: "Departamentos",
                column: "Activo");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Puestos_PuestoId",
                table: "Empleados",
                column: "PuestoId",
                principalTable: "Puestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Puestos_PuestoId",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Puestos_DepartamentoId_Activo",
                table: "Puestos");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_Correo",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_DPI",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_NIT",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Departamentos_Activo",
                table: "Departamentos");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Puestos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Puestos",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PuestoId",
                table: "Empleados",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NombreCompleto",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NIT",
                table: "Empleados",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Empleados",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EstadoLaboral",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "ACTIVO");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DPI",
                table: "Empleados",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Empleados",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Departamentos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Departamentos",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Puestos_DepartamentoId",
                table: "Puestos",
                column: "DepartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Correo",
                table: "Empleados",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_DPI",
                table: "Empleados",
                column: "DPI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_NIT",
                table: "Empleados",
                column: "NIT",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Puestos_PuestoId",
                table: "Empleados",
                column: "PuestoId",
                principalTable: "Puestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
