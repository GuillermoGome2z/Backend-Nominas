using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoNomina.Backend.Migrations
{
    /// <inheritdoc />
    public partial class PayrollSystemComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Anio",
                table: "Nominas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CerradoEn",
                table: "Nominas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "Nominas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCorte",
                table: "Nominas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Mes",
                table: "Nominas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quincena",
                table: "Nominas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPeriodo",
                table: "Nominas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Comisiones",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentosVarios",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtras",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasOrdinarias",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IgssEmpleado",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Isr",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LiquidoAPagar",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoHorasExtras",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtrosIngresos",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeducciones",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDevengado",
                table: "DetalleNominas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ConceptosNomina",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Formula = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AfectaIgss = table.Column<bool>(type: "bit", nullable: false),
                    AfectaIsr = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CuentaContable = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosNomina", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmpleadoParametros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    AfiliadoIgss = table.Column<bool>(type: "bit", nullable: false),
                    NumeroIgss = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaAfiliacionIgss = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExentoIsr = table.Column<bool>(type: "bit", nullable: false),
                    MotivoExencion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    JornadaMensualHoras = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalarioHora = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CuentaBancaria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BancoNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TipoCuenta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FormaPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecibeBonoDecreto = table.Column<bool>(type: "bit", nullable: false),
                    DescuentosFijosMensuales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ObservacionesDescuentos = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VigenteDesde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VigenteHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpleadoParametros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpleadoParametros_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NominaAportesPatronales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NominaId = table.Column<int>(type: "int", nullable: false),
                    TotalIgssPatronal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalIrtra = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalIntecap = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAguinaldo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalBono14 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalVacaciones = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalIndemnizacion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAportesPatronales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DetalleJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CalculadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NominaAportesPatronales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NominaAportesPatronales_Nominas_NominaId",
                        column: x => x.NominaId,
                        principalTable: "Nominas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NominaDetalleLineas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NominaDetalleId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodigoConcepto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Base = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tasa = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EsManual = table.Column<bool>(type: "bit", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NominaDetalleLineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NominaDetalleLineas_DetalleNominas_NominaDetalleId",
                        column: x => x.NominaDetalleId,
                        principalTable: "DetalleNominas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReglasLaborales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pais = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    VigenteDesde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VigenteHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IgssEmpleadoPct = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IgssPatronalPct = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IrtraPct = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IntecapPct = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IsrEscalaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HorasExtrasPct = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    HorasExtrasNocturnasPct = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    BonoDecretoMonto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RedondeoDecimales = table.Column<int>(type: "int", nullable: false),
                    PoliticaRedondeo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SalarioMinimoMensual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    JornadaOrdinariaHorasMes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasLaborales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosNomina_Codigo",
                table: "ConceptosNomina",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosNomina_Tipo_Activo",
                table: "ConceptosNomina",
                columns: new[] { "Tipo", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoParametros_EmpleadoId",
                table: "EmpleadoParametros",
                column: "EmpleadoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoParametros_EmpleadoId_Activo",
                table: "EmpleadoParametros",
                columns: new[] { "EmpleadoId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoParametros_VigenteDesde",
                table: "EmpleadoParametros",
                column: "VigenteDesde");

            migrationBuilder.CreateIndex(
                name: "IX_NominaAportesPatronales_NominaId",
                table: "NominaAportesPatronales",
                column: "NominaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NominaDetalleLineas_NominaDetalleId_Tipo_Orden",
                table: "NominaDetalleLineas",
                columns: new[] { "NominaDetalleId", "Tipo", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_ReglasLaborales_Pais_VigenteDesde_Activo",
                table: "ReglasLaborales",
                columns: new[] { "Pais", "VigenteDesde", "Activo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConceptosNomina");

            migrationBuilder.DropTable(
                name: "EmpleadoParametros");

            migrationBuilder.DropTable(
                name: "NominaAportesPatronales");

            migrationBuilder.DropTable(
                name: "NominaDetalleLineas");

            migrationBuilder.DropTable(
                name: "ReglasLaborales");

            migrationBuilder.DropColumn(
                name: "Anio",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "CerradoEn",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "CreadoPor",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "FechaCorte",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "Mes",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "Quincena",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "TipoPeriodo",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "Comisiones",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "DescuentosVarios",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "HorasExtras",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "HorasOrdinarias",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "IgssEmpleado",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "Isr",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "LiquidoAPagar",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "MontoHorasExtras",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "OtrosIngresos",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "TotalDeducciones",
                table: "DetalleNominas");

            migrationBuilder.DropColumn(
                name: "TotalDevengado",
                table: "DetalleNominas");
        }
    }
}
