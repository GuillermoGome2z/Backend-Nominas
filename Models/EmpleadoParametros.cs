using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoNomina.Backend.Models
{
    /// <summary>
    /// Parámetros específicos de nómina por empleado
    /// Configuración de afiliación IGSS, exención ISR, datos bancarios, etc.
    /// </summary>
    [Table("EmpleadoParametros")]
    public class EmpleadoParametros
    {
        public int Id { get; set; }

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; } = null!;

        // ===== IGSS =====
        public bool AfiliadoIgss { get; set; } = true;

        [MaxLength(20)]
        public string? NumeroIgss { get; set; }

        public DateTime? FechaAfiliacionIgss { get; set; }

        // ===== ISR =====
        public bool ExentoIsr { get; set; } = false;

        [MaxLength(200)]
        public string? MotivoExencion { get; set; }

        // ===== Jornada laboral =====
        /// <summary>
        /// Horas de jornada mensual (para cálculo de tarifa por hora)
        /// Default: 173.33 horas (~40h/semana * 4.33 semanas/mes)
        /// </summary>
        [Precision(18, 2)]
        public decimal JornadaMensualHoras { get; set; } = 173.33m;

        /// <summary>
        /// Tarifa por hora = SalarioBase / JornadaMensualHoras
        /// Se calcula automáticamente
        /// </summary>
        [Precision(18, 4)]
        public decimal SalarioHora { get; set; }

        // ===== Datos bancarios =====
        [MaxLength(50)]
        public string? CuentaBancaria { get; set; }

        [MaxLength(100)]
        public string? BancoNombre { get; set; }

        [MaxLength(20)]
        public string? TipoCuenta { get; set; } // "Ahorro" | "Monetaria"

        // ===== Configuraciones adicionales =====
        /// <summary>
        /// Forma de pago: "Efectivo" | "Transferencia" | "Cheque"
        /// </summary>
        [MaxLength(20)]
        public string FormaPago { get; set; } = "Transferencia";

        /// <summary>
        /// Indica si recibe bono decreto (Q250 mensuales en Guatemala)
        /// </summary>
        public bool RecibeBonoDecreto { get; set; } = true;

        /// <summary>
        /// Descuentos fijos mensuales (préstamos, anticipos, etc.)
        /// </summary>
        [Precision(18, 2)]
        public decimal DescuentosFijosMensuales { get; set; } = 0m;

        [MaxLength(500)]
        public string? ObservacionesDescuentos { get; set; }

        // ===== Vigencia =====
        [Required]
        public DateTime VigenteDesde { get; set; }

        public DateTime? VigenteHasta { get; set; }

        public bool Activo { get; set; } = true;

        // Auditoría
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreadoPor { get; set; }

        public DateTime? ModificadoEn { get; set; }

        [MaxLength(100)]
        public string? ModificadoPor { get; set; }
    }
}
