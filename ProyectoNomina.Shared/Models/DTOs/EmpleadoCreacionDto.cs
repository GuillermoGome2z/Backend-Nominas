using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class EmpleadoCreacionDto
    {
        
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public decimal SalarioBase { get; set; }
        public int DepartamentoId { get; set; }
        public int PuestoId { get; set; }
        public DateTime FechaContratacion { get; set; }
        public string DPI { get; set; }
        public string NIT{ get; set;}
        public DateTime FechaNacimiento { get; set; }
        public string EstadoLaboral { get; set; } = string.Empty;
    }
}
