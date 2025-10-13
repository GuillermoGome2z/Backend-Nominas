using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class ReporteNominaDto
    {
        public int NominaId { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public decimal TotalSalarios { get; set; }
        public decimal TotalBonificaciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal TotalNeto { get; set; }
    }
}
