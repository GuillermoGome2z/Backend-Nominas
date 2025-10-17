using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public sealed class DocumentoEmpleadoDto
    {
        public int Id { get; init; }
        [Required] public int EmpleadoId { get; init; }
        [Required] public int TipoDocumentoId { get; init; }

        // Opcionales: pueden ser null cuando no se incluye la navegación
        public string? NombreTipo { get; init; }
        public string? NombreEmpleado { get; init; }

        [Required] public string RutaArchivo { get; init; } = string.Empty;
        public DateTime FechaSubida { get; init; }
    }
}
