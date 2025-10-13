using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class DocumentoSubidaDto
    {
        public int EmpleadoId { get; set; }
        public int TipoDocumentoId { get; set; }
        public IFormFile Archivo { get; set; } = default!;
    }
}
