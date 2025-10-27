using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Backend.Models
{
    /// <summary>
    /// Tabla de configuración de reglas laborales vigentes por país
    /// Almacena tasas de IGSS, ISR, aportes patronales, etc.
    /// </summary>
    public class ReglasLaborales
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(2)]
        public string Pais { get; set; } = "GT"; // Guatemala

        [Required]
        public DateTime VigenteDesde { get; set; }

        public DateTime? VigenteHasta { get; set; }

        // ===== Tasas IGSS =====
        [Precision(18, 6)]
        public decimal IgssEmpleadoPct { get; set; } // 4.83% para Guatemala

        [Precision(18, 6)]
        public decimal IgssPatronalPct { get; set; } // 10.67% para Guatemala

        // ===== Aportes patronales adicionales =====
        [Precision(18, 6)]
        public decimal IrtraPct { get; set; } // IRTRA 1%

        [Precision(18, 6)]
        public decimal IntecapPct { get; set; } // INTECAP 1%

        // ===== ISR - Tabla progresiva en JSON =====
        /// <summary>
        /// Escala progresiva de ISR en formato JSON
        /// Ejemplo: [{"desde":0,"hasta":300000,"tasa":0.05,"excesoSobre":0,"impuestoFijo":0}, ...]
        /// </summary>
        [Required]
        public string IsrEscalaJson { get; set; } = "[]";

        // ===== Configuración horas extra =====
        [Precision(18, 6)]
        public decimal HorasExtrasPct { get; set; } = 1.5m; // 150% = 1.5

        [Precision(18, 6)]
        public decimal HorasExtrasNocturnasPct { get; set; } = 2.0m; // 200% = 2.0

        // ===== Bono Decreto 37-2001 (Guatemala) =====
        [Precision(18, 2)]
        public decimal BonoDecretoMonto { get; set; } = 250m; // Q250.00

        // ===== Configuración de redondeo =====
        public int RedondeoDecimales { get; set; } = 2;

        [Required]
        [MaxLength(10)]
        public string PoliticaRedondeo { get; set; } = "Normal"; // "Arriba" | "Normal" | "Abajo"

        // ===== Configuraciones adicionales =====
        [Precision(18, 2)]
        public decimal SalarioMinimoMensual { get; set; } = 3000m; // Q3,000.00 aprox

        [Precision(18, 2)]
        public decimal JornadaOrdinariaHorasMes { get; set; } = 173.33m; // ~40h/semana * 4.33

        public bool Activo { get; set; } = true;

        // Auditoría
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreadoPor { get; set; }

        public DateTime? ModificadoEn { get; set; }

        [MaxLength(100)]
        public string? ModificadoPor { get; set; }
    }
}
