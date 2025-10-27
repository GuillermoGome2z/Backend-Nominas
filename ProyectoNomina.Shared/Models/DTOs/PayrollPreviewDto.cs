namespace ProyectoNomina.Shared.Models.DTOs
{
    /// <summary>
    /// DTO de respuesta para simulación/preview de nómina
    /// </summary>
    public class PayrollPreviewDto
    {
        public string TipoPeriodo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int? Quincena { get; set; }
        public DateTime FechaCorte { get; set; }

        public List<EmpleadoNominaDto> Empleados { get; set; } = new();

        // Totales generales
        public decimal TotalDevengado { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal TotalLiquido { get; set; }

        // Aportes patronales (informativos, no se descuentan al empleado)
        public decimal TotalIgssPatronal { get; set; }
        public decimal TotalIrtra { get; set; }
        public decimal TotalIntecap { get; set; }
        public decimal TotalAportesPatronales { get; set; }

        // Resumen
        public int TotalEmpleados { get; set; }
        public string? Observaciones { get; set; }

        // Propiedades adicionales requeridas por PayrollService
        public string TipoNomina { get; set; } = "ORDINARIA";
        public string Periodo { get; set; } = string.Empty;
        public decimal TotalBruto { get; set; }
        public decimal TotalNeto { get; set; }
        public int CantidadEmpleados { get; set; }
    }

    /// <summary>
    /// Detalle de nómina por empleado
    /// </summary>
    public class EmpleadoNominaDto
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? NumeroIgss { get; set; }
        public string? Departamento { get; set; }
        public string? Puesto { get; set; }

        // Percepciones
        public decimal SalarioBase { get; set; }
        public decimal HorasOrdinarias { get; set; }
        public decimal HorasExtras { get; set; }
        public decimal MontoHorasExtras { get; set; }
        public decimal Bonificaciones { get; set; }
        public decimal Comisiones { get; set; }
        public decimal OtrosIngresos { get; set; }
        public decimal TotalDevengado { get; set; }

        // Deducciones
        public decimal IgssEmpleado { get; set; }
        public decimal Isr { get; set; }
        public decimal DescuentosVarios { get; set; }
        public decimal TotalDeducciones { get; set; }

        public decimal LiquidoAPagar { get; set; }

        // Desglose detallado línea por línea
        public List<LineaCalculoDto> Percepciones { get; set; } = new();
        public List<LineaCalculoDto> Deducciones { get; set; } = new();

        // Aportes patronales (no van al empleado, solo info)
        public decimal IgssPatronal { get; set; }
        public decimal Irtra { get; set; }
        public decimal Intecap { get; set; }

        // Propiedades adicionales requeridas por PayrollService
        public decimal SalarioBruto { get; set; }
        public decimal SalarioNeto { get; set; }
        public decimal Prestamos { get; set; }
        public decimal Anticipos { get; set; }
        public decimal OtrasDeducciones { get; set; }
    }

    /// <summary>
    /// Línea individual de cálculo (percepción o deducción)
    /// </summary>
    public class LineaCalculoDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Base { get; set; }
        public decimal? Tasa { get; set; }
        public decimal Monto { get; set; }
        public bool EsManual { get; set; }
        public string? Observaciones { get; set; }
    }
}
