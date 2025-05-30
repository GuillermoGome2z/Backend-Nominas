using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class DocumentoEmpleadoDto
    {
        public int Id { get; set; }

        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;

        public int TipoDocumentoId { get; set; }
        public string NombreTipo { get; set; } = string.Empty;

        public string RutaArchivo { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
    }
}
