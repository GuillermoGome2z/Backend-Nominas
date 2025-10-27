using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoNomina.Backend.Models
{
    /// <summary>
    /// Desglose línea por línea de percepciones y deducciones de cada empleado
    /// Permite trazabilidad completa de cómo se calculó la nómina
    /// </summary>
    [Table("NominaDetalleLineas")]
    public class NominaDetalleLinea
    {
        public int Id { get; set; }

        public int NominaDetalleId { get; set; }
        public DetalleNomina NominaDetalle { get; set; } = null!;

        /// <summary>
        /// Tipo de línea: "Percepcion" | "Deduccion"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Código del concepto: SALARIO_BASE, IGSS, ISR, HORAS_EXTRAS, etc.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CodigoConcepto { get; set; } = string.Empty;

        /// <summary>
        /// Descripción legible del concepto
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Base de cálculo (monto sobre el que se aplica la tasa)
        /// </summary>
        [Precision(18, 2)]
        public decimal Base { get; set; }

        /// <summary>
        /// Tasa aplicada (porcentaje en decimal, ej: 0.0483 para 4.83%)
        /// Null si no aplica tasa (montos fijos)
        /// </summary>
        [Precision(18, 6)]
        public decimal? Tasa { get; set; }

        /// <summary>
        /// Monto final de la línea
        /// </summary>
        [Precision(18, 2)]
        public decimal Monto { get; set; }

        /// <summary>
        /// Indica si es un ajuste manual (true) o calculado automáticamente (false)
        /// </summary>
        public bool EsManual { get; set; } = false;

        /// <summary>
        /// Observaciones adicionales para ajustes manuales
        /// </summary>
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Orden de despliegue en reportes
        /// </summary>
        public int Orden { get; set; }

        // Auditoría
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreadoPor { get; set; }
    }
}
