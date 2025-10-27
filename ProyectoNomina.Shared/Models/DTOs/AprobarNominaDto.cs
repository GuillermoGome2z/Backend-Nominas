using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    /// <summary>
    /// DTO para aprobar una nómina
    /// </summary>
    public class AprobarNominaDto
    {
        /// <summary>
        /// Indica si la nómina fue aprobada
        /// </summary>
        [Required]
        public bool Aprobada { get; set; }

        /// <summary>
        /// Observaciones adicionales al aprobar (opcional)
        /// </summary>
        [MaxLength(1000)]
        public string? Observaciones { get; set; }
    }
}
