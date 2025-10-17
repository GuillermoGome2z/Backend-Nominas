using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoNomina.Backend.Models
{
    [Table("DetalleNominas")] 
    public class DetalleNomina
    {
        public int Id { get; set; }

        public int NominaId { get; set; }
        public Nomina Nomina { get; set; } = null!;

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; } = null!;

        // ===== NUEVO: horas y tarifas =====
        [Precision(18, 2)]
        public decimal HorasRegulares { get; set; }

        [Precision(18, 2)]
        public decimal HorasExtra { get; set; }

        [Precision(18, 2)]
        public decimal TarifaHora { get; set; }

        [Precision(18, 2)]
        public decimal TarifaExtra { get; set; }

        // ===== Ya existentes =====
        [Precision(18, 2)]
        public decimal SalarioBruto { get; set; }

        [Precision(18, 2)]
        public decimal Deducciones { get; set; }

        [Precision(18, 2)]
        public decimal Bonificaciones { get; set; }

        [Precision(18, 2)]
        public decimal SalarioNeto { get; set; }

        public string DesgloseDeducciones { get; set; } = string.Empty;
    }
}
