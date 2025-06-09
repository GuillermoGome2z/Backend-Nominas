using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class DocumentoEmpleadoCreateDto
    {
        public int TipoDocumentoId { get; set; }
        public string RutaArchivo { get; set; } = string.Empty;
        public int EmpleadoId { get; set; }
    }
}
