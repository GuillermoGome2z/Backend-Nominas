using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class ReporteDocumentosEmpleadoDto
    {
        public string NombreEmpleado { get; set; } = string.Empty;
        public List<ItemDocumentoResumenDto> Documentos { get; set; } = new();
    }
}
