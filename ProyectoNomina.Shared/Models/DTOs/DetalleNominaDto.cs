using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class DetalleNominaDto
    {
        public int Id { get; set; }
        public int NominaId { get; set; }
        public int EmpleadoId { get; set; }
        public decimal SalarioBruto { get; set; }
        public decimal Deducciones { get; set; }
        public decimal Bonificaciones { get; set; }
        public decimal SalarioNeto { get; set; }
        public string DesgloseDeducciones { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
       
     

    }
}
