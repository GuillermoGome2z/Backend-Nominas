using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class AuditoriaDto
    {
        public int Id { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Detalles { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }
}
