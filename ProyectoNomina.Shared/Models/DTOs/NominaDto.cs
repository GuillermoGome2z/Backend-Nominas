using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class NominaDto
    {
        public int Id { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public List<DetalleNominaDto> Detalles { get; set; } = new();
    }
}
