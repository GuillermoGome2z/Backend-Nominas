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

        // Estados de la nómina
        public string Estado { get; set; } = "BORRADOR"; // BORRADOR, PENDIENTE, APROBADA, PAGADA, ANULADA

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
        
        // Alias para compatibilidad con código existente
        [NotMapped]
        public ICollection<DetalleNomina> Detalles => DetallesNomina;
    }
}
