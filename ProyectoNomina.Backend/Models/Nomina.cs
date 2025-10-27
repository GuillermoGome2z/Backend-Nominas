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
        public string? Periodo { get; set; } // Ej: "2025-01", "2025-02"
        
        // Campos adicionales para control de período
        [Required]
        [MaxLength(20)]
        public string TipoPeriodo { get; set; } = "Mensual"; // "Mensual" | "Quincenal"
        
        [Required]
        public int Anio { get; set; }
        
        [Required]
        [Range(1, 12)]
        public int Mes { get; set; }
        
        [Range(1, 2)]
        public int? Quincena { get; set; } // 1 o 2, solo para TipoPeriodo = "Quincenal"
        
        [Required]
        public DateTime FechaCorte { get; set; }
        
        [MaxLength(100)]
        public string? CreadoPor { get; set; }
        
        public DateTime? CerradoEn { get; set; }

        // Estados de la nómina
        public string Estado { get; set; } = "BORRADOR"; // BORRADOR, PROCESADA, CERRADA

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

        // Relaciones
        public ICollection<DetalleNomina> DetallesNomina { get; set; } = new List<DetalleNomina>();
        
        // Aportes patronales (1:1)
        public NominaAportesPatronales? AportesPatronales { get; set; }
        
        // Alias para compatibilidad con código existente
        [NotMapped]
        public ICollection<DetalleNomina> Detalles => DetallesNomina;
    }
}
