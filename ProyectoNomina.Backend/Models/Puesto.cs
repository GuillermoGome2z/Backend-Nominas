using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using ProyectoNomina.Shared.Models.DTOs;


namespace ProyectoNomina.Backend.Models
{
    public class Puesto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        [Precision(18, 2)]
        public decimal SalarioBase { get; set; }

        public ICollection<Empleado> Empleados { get; set; }
    }
}

