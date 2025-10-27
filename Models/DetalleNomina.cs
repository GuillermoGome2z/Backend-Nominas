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
        
        // ===== Desgloses detallados de percepciones =====
        [Precision(18, 2)]
        public decimal HorasOrdinarias { get; set; }
        
        [Precision(18, 2)]
        public decimal HorasExtras { get; set; }
        
        [Precision(18, 2)]
        public decimal MontoHorasExtras { get; set; }
        
        [Precision(18, 2)]
        public decimal Comisiones { get; set; }
        
        [Precision(18, 2)]
        public decimal OtrosIngresos { get; set; }
        
        // ===== Desgloses detallados de deducciones =====
        [Precision(18, 2)]
        public decimal IgssEmpleado { get; set; }
        
        [Precision(18, 2)]
        public decimal Isr { get; set; }
        
        [Precision(18, 2)]
        public decimal DescuentosVarios { get; set; }
        
        // ===== Deducciones específicas =====
        [Precision(18, 2)]
        public decimal Prestamos { get; set; }
        
        [Precision(18, 2)]
        public decimal Anticipos { get; set; }
        
        [Precision(18, 2)]
        public decimal OtrasDeducciones { get; set; }
        
        [Precision(18, 2)]
        public decimal TotalDevengado { get; set; }
        
        [Precision(18, 2)]
        public decimal TotalDeducciones { get; set; }
        
        [Precision(18, 2)]
        public decimal LiquidoAPagar { get; set; }

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
        
        // Relación con líneas de detalle
        public ICollection<NominaDetalleLinea> Lineas { get; set; } = new List<NominaDetalleLinea>();
    }
}
