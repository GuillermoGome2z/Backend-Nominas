using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProyectoNomina.Backend.Models
{
    public class AjusteManual
    {
        public int Id { get; set; }

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; } = null!;

        [Precision(18, 2)]
        public decimal Monto { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
    }
}
