using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class EmpleadoDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DPI { get; set; } = string.Empty;
        public string NIT { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaContratacion { get; set; }
        public decimal SalarioMensual { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string EstadoLaboral { get; set; }


        public int DepartamentoId { get; set; }
        public int PuestoId { get; set; }

        // Opcional
        public string? NombreDepartamento { get; set; }
        public string? NombrePuesto { get; set; }
    }
}
