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
         [Required(ErrorMessage = "El empleado es obligatorio")]
        public string Empleado { get; set; } 
         [Required(ErrorMessage = "El estado del expediente es obligatorio")]
        public string EstadoExpediente { get; set; } 
        public int DocumentosRequeridos { get; set; }
        public int DocumentosPresentados { get; set; }
        public int DocumentosFaltantes { get; set; }
    }
}
