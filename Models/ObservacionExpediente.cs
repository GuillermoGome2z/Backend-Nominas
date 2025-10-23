using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Backend.Models
{
    // Observación general del expediente o asociada a un documento puntual
    public class ObservacionExpediente
    {
        public int Id { get; set; }

        [Required]
        public int EmpleadoId { get; set; }

        // Si se desea comentar un documento específico del expediente
        public int? DocumentoEmpleadoId { get; set; }

        [Required, MaxLength(2000)]
        public string Texto { get; set; } = string.Empty;

        // Auditoría mínima
        [Required]
        public int UsuarioId { get; set; }

        public DateTime FechaCreacion { get; set; }     // UTC
        public DateTime? FechaActualizacion { get; set; } // UTC
    }
}
