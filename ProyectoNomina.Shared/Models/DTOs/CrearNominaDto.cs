using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class CrearNominaDto
    {
        [Required(ErrorMessage = "El período es requerido")]
        [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$", 
            ErrorMessage = "El período debe tener formato YYYY-MM (ej: 2025-10)")]
        public string Periodo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de nómina es requerido")]
        public string TipoNomina { get; set; } = string.Empty;
        
        public DateTime? FechaCorte { get; set; }

        public List<int>? DepartamentoIds { get; set; }
        
        public List<int>? EmpleadoIds { get; set; }

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres")]
        public string? Observaciones { get; set; }
    }
}
