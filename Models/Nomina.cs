using ProyectoNomina.Shared.Models.DTOs;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Backend.Models
{
    public class Nomina
    {
        public int Id { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // Propiedades para el período de la nómina
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        
        // Periodo en formato "2025-10"
        [MaxLength(20)]
        public string? Periodo { get; set; }
        
        // Campos adicionales para control de período
        public int? Anio { get; set; }
        
        [Range(1, 12)]
        public int? Mes { get; set; }
        
        [Range(1, 2)]
        public int? Quincena { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string TipoPeriodo { get; set; } = "Mensual"; // "Mensual" | "Quincenal"
        
        [Required]
        public DateTime FechaCorte { get; set; }
        
        // Tipo de nómina
        [Required]
        [MaxLength(30)]
        public string TipoNomina { get; set; } = "ORDINARIA"; // ORDINARIA, EXTRAORDINARIA, AGUINALDO, BONO14
        
        // Estados de la nómina
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "BORRADOR"; // BORRADOR, APROBADA, PAGADA, ANULADA

        // Fechas de control de estado
        public DateTime? FechaAprobacion { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public string? MotivoAnulacion { get; set; }

        // Montos totales calculados
        [Precision(18, 2)]
        public decimal MontoTotal { get; set; }

        [Precision(18, 2)]
        public decimal TotalBruto { get; set; }

        [Precision(18, 2)]
        public decimal TotalDeducciones { get; set; }

        [Precision(18, 2)]
        public decimal TotalBonificaciones { get; set; }

        [Precision(18, 2)]
        public decimal TotalNeto { get; set; }
        
        // Totales de deducciones específicas (suma de todos los empleados)
        [Precision(18, 2)]
        public decimal TotalIgssEmpleado { get; set; }
        
        [Precision(18, 2)]
        public decimal TotalIsr { get; set; }
        
        // Cantidad de empleados en la nómina
        public int CantidadEmpleados { get; set; }
        
        // Control de usuarios
        [MaxLength(100)]
        public string? CreadoPor { get; set; }
        
        [MaxLength(100)]
        public string? AprobadoPor { get; set; }
        
        [MaxLength(1000)]
        public string? Observaciones { get; set; }
        
        public DateTime? CerradoEn { get; set; }

        // Relaciones
        public ICollection<DetalleNomina> DetallesNomina { get; set; } = new List<DetalleNomina>();
        
        // Aportes patronales (1:1)
        public NominaAportesPatronales? AportesPatronales { get; set; }
        
        // Alias para compatibilidad con código existente
        [NotMapped]
        public ICollection<DetalleNomina> Detalles => DetallesNomina;
    }
}
