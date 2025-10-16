using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class ObservacionExpedienteCreateDto
    {
        // Opcional: para asociarla a un documento (si no se envía, queda a nivel expediente)
        public int? DocumentoEmpleadoId { get; set; }

        [Required, MaxLength(2000)]
        public string Texto { get; set; } = string.Empty;
    }
}
