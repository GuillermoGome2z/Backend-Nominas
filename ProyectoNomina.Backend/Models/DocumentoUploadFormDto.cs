using Microsoft.AspNetCore.Http;
using ProyectoNomina.Backend.Models.DTOs;

namespace ProyectoNomina.Backend.Models.DTOs
{
    public class DocumentoUploadFormDto
    {
        public int TipoDocumentoId { get; set; }
        public IFormFile Archivo { get; set; } = default!;
    }
}
