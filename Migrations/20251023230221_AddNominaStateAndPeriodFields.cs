using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNominaStateAndPeriodFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Nominas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "Nominas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "Nominas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFin",
                table: "Nominas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicio",
                table: "Nominas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPago",
                table: "Nominas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoTotal",
                table: "Nominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "Nominas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Periodo",
                table: "Nominas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBonificaciones",
                table: "Nominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBruto",
                table: "Nominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeducciones",
                table: "Nominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNeto",
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
                name: "Estado",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "FechaFin",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "FechaInicio",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "FechaPago",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "MontoTotal",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "Periodo",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "TotalBonificaciones",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "TotalBruto",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "TotalDeducciones",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "TotalNeto",
                table: "Nominas");
        }
    }
}
