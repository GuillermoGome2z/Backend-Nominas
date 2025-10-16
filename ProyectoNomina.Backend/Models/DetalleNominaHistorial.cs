using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoNomina.Backend.Models
{
    [Table("DetalleNominaHistorial")]
    public class DetalleNominaHistorial
    {
        public int Id { get; set; }

        public int DetalleNominaId { get; set; }
        public DetalleNomina? DetalleNomina { get; set; }

        [MaxLength(128)]
        public string Campo { get; set; } = string.Empty;

        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }

        [MaxLength(128)]
        public string? UsuarioId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
