using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Detalles { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Metodo { get; set; } = string.Empty;
    }
}
