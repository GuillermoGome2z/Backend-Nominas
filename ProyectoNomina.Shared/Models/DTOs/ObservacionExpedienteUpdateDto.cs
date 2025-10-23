using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class ObservacionExpedienteUpdateDto
    {
        [Required, MaxLength(2000)]
        public string Texto { get; set; } = string.Empty;
    }
}
