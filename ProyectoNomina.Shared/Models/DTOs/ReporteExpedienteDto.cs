using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class ReporteExpedienteDto
    {
        public string Empleado { get; set; }
        public string EstadoExpediente { get; set; }
        public int DocumentosRequeridos { get; set; }
        public int DocumentosPresentados { get; set; }
        public int DocumentosFaltantes { get; set; }
    }
}
