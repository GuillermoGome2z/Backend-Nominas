using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Backend.Models
{
    /// <summary>
    /// Catálogo de conceptos configurables para nómina
    /// Permite definir percepciones, deducciones y aportes patronales personalizados
    /// </summary>
    public class ConceptoNomina
    {
        public int Id { get; set; }

        /// <summary>
        /// Código único del concepto: SALARIO_BASE, HORAS_EXTRAS, IGSS, ISR, etc.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo del concepto
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de concepto: "Percepcion" | "Deduccion" | "AportePatronal"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Fórmula o expresión para cálculo automático (opcional)
        /// Puede usar placeholders como {SalarioBase}, {HorasExtras}, etc.
        /// </summary>
        [MaxLength(500)]
        public string? Formula { get; set; }

        /// <summary>
        /// Indica si este concepto afecta la base para cálculo de IGSS
        /// </summary>
        public bool AfectaIgss { get; set; } = true;

        /// <summary>
        /// Indica si este concepto afecta la renta imponible para ISR
        /// </summary>
        public bool AfectaIsr { get; set; } = true;

        /// <summary>
        /// Indica si el concepto está activo y disponible
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Orden de visualización en reportes
        /// </summary>
        public int Orden { get; set; }

        /// <summary>
        /// Descripción adicional o ayuda del concepto
        /// </summary>
        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Cuenta contable asociada (opcional, para integración contable)
        /// </summary>
        [MaxLength(20)]
        public string? CuentaContable { get; set; }

        // Auditoría
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreadoPor { get; set; }

        public DateTime? ModificadoEn { get; set; }

        [MaxLength(100)]
        public string? ModificadoPor { get; set; }
    }
}
