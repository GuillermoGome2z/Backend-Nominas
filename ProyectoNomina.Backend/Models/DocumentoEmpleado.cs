using ProyectoNomina.Shared.Models.DTOs;
namespace ProyectoNomina.Backend.Models
{
    public class DocumentoEmpleado
    {
        public int Id { get; set; }

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; }

        public int TipoDocumentoId { get; set; }
        public TipoDocumento TipoDocumento { get; set; }

        public string RutaArchivo { get; set; }
        public DateTime FechaSubida { get; set; }
    }
}