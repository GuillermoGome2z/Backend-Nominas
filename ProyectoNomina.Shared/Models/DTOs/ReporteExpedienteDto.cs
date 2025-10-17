using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
   public class ReporteExpedienteDto
{
    [Required] public required string Empleado { get; init; }
    [Required] public required string EstadoExpediente { get; init; }
    public int DocumentosRequeridos { get; init; }
    public int DocumentosPresentados { get; init; }
    public int DocumentosFaltantes { get; init; }
}
}
