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
        
        // ===== CAMPOS OBLIGATORIOS PARA EXPORTACIÓN =====
        public string NombreEmpleado { get; set; } = string.Empty;
        public string? NombreDepartamento { get; set; }
        public string? NombrePuesto { get; set; }
        public decimal SalarioBase { get; set; } // Salario base mensual del empleado
        public decimal BonoDecreto { get; set; } // Bono Decreto 37-2001 Q250
        public decimal TotalDevengado { get; set; } // Total de ingresos
        public decimal Igss { get; set; } // Descuento IGSS 4.83%
        public decimal Isr { get; set; } // Descuento ISR calculado
        public decimal TotalDeducciones { get; set; } // Suma de todas las deducciones
        public decimal SalarioNeto { get; set; } // Salario final después de deducciones

        // ===== CAMPOS OPCIONALES PERO IMPORTANTES =====
        public decimal Bonificaciones { get; set; } // Otras bonificaciones
        public decimal Comisiones { get; set; } // Comisiones ganadas
        public decimal HorasExtraValor { get; set; } // Valor de horas extras
        public decimal Prestamos { get; set; } // Descuentos por préstamos
        public decimal Anticipos { get; set; } // Descuentos por anticipos
        public decimal OtrasDeducciones { get; set; } // Otras deducciones
        public decimal BaseIgssCalculada { get; set; } // Base sobre la cual se calculó el IGSS
        public bool ExencionAplicada { get; set; } // Si se aplicó exención (Aguinaldo/Bono14)

        // ===== CAMPOS DE COMPATIBILIDAD (LEGACY) =====
        public decimal SalarioBruto { get; set; }
        public decimal Deducciones { get; set; }
        public string DesgloseDeducciones { get; set; } = string.Empty;

        // ===== CAMPOS INFORMATIVOS ADICIONALES =====
        public string? TipoNomina { get; set; } // ORDINARIA, EXTRAORDINARIA, AGUINALDO, BONO14
        public DateTime? FechaProceso { get; set; } // Fecha de procesamiento
        public decimal HorasOrdinarias { get; set; } // Horas trabajadas normales
        public decimal HorasExtras { get; set; } // Horas extras trabajadas
        public decimal TarifaHora { get; set; } // Tarifa por hora del empleado
    }
}
