using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoNomina.Backend.Models
{
    /// <summary>
    /// Registro de aportes patronales por nómina
    /// Estos montos NO se descuentan al empleado, son costos del empleador
    /// Útil para reportes contables y presupuestarios
    /// </summary>
    [Table("NominaAportesPatronales")]
    public class NominaAportesPatronales
    {
        public int Id { get; set; }

        public int NominaId { get; set; }
        public Nomina Nomina { get; set; } = null!;

        // ===== Aportes patronales =====
        /// <summary>
        /// IGSS patronal (10.67% en Guatemala)
        /// </summary>
        [Precision(18, 2)]
        public decimal TotalIgssPatronal { get; set; }

        /// <summary>
        /// IRTRA - Instituto Recreación Trabajadores (1%)
        /// </summary>
        [Precision(18, 2)]
        public decimal TotalIrtra { get; set; }

        /// <summary>
        /// INTECAP - Instituto Técnico Capacitación (1%)
        /// </summary>
        [Precision(18, 2)]
        public decimal TotalIntecap { get; set; }

        /// <summary>
        /// Aguinaldo proporcional acumulado (si aplica)
        /// </summary>
        [Precision(18, 2)]
        public decimal TotalAguinaldo { get; set; }

        /// <summary>
        /// Bono 14 proporcional acumulado (si aplica)
        /// </summary>
        [Precision(18, 2)]
        public decimal TotalBono14 { get; set; }

        /// <summary>
        /// Vacaciones proporcionales acumuladas
        /// </summary>
        [Precision(18, 2)]
        public decimal TotalVacaciones { get; set; }

        /// <summary>
        /// Indemnización proporcional acumulada
        /// </summary>
        [Precision(18, 2)]
        public decimal TotalIndemnizacion { get; set; }

        /// <summary>
        /// Suma de todos los aportes patronales
        /// </summary>
        [Precision(18, 2)]
        public decimal TotalAportesPatronales { get; set; }

        /// <summary>
        /// Desglose detallado en JSON (opcional)
        /// </summary>
        public string? DetalleJson { get; set; }

        // Auditoría
        public DateTime CalculadoEn { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CalculadoPor { get; set; }
    }
}
