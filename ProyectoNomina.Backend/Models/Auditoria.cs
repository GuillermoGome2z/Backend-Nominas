using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Models
{
    public class Auditoria
    {
        public int Id { get; set; } // Identificador único del registro de auditoría

        public string Accion { get; set; } = null!; // Acción realizada (GET, POST, PUT, DELETE, etc.)

        public string Usuario { get; set; } = null!; // Usuario autenticado que realizó la acción

        public DateTime Fecha { get; set; } // Fecha y hora de la acción

        public string Detalles { get; set; } = null!; // Información detallada de lo que se hizo

        public string Endpoint { get; set; } = null!; // Ruta o endpoint donde ocurrió la acción
    }
}
