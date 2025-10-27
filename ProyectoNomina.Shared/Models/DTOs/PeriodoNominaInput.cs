using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    /// <summary>
    /// DTO de entrada para solicitar simulación o procesamiento de nómina
    /// </summary>
    public class PeriodoNominaInput
    {
        /// <summary>
        /// Tipo de período: "Mensual" | "Quincenal"
        /// </summary>
        [Required(ErrorMessage = "El tipo de período es requerido")]
        [RegularExpression("^(Mensual|Quincenal)$", ErrorMessage = "Debe ser 'Mensual' o 'Quincenal'")]
        public string TipoPeriodo { get; set; } = "Mensual";

        /// <summary>
        /// Año de la nómina
        /// </summary>
        [Required]
        [Range(2020, 2100, ErrorMessage = "Año debe estar entre 2020 y 2100")]
        public int Anio { get; set; }

        /// <summary>
        /// Mes de la nómina (1-12)
        /// </summary>
        [Required]
        [Range(1, 12, ErrorMessage = "Mes debe estar entre 1 y 12")]
        public int Mes { get; set; }

        /// <summary>
        /// Quincena (1 o 2), solo requerido si TipoPeriodo = "Quincenal"
        /// </summary>
        [Range(1, 2, ErrorMessage = "Quincena debe ser 1 o 2")]
        public int? Quincena { get; set; }

        /// <summary>
        /// Fecha de corte para aplicar reglas laborales vigentes
        /// Si no se especifica, se usa la fecha actual
        /// </summary>
        public DateTime? FechaCorte { get; set; }

        /// <summary>
        /// Filtrar por departamentos específicos
        /// </summary>
        public List<int>? DepartamentoIds { get; set; }

        /// <summary>
        /// Filtrar por empleados específicos
        /// </summary>
        public List<int>? EmpleadoIds { get; set; }

        /// <summary>
        /// Incluir solo empleados activos
        /// </summary>
        public bool SoloActivos { get; set; } = true;
    }
}
