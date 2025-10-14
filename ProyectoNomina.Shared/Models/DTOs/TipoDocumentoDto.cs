using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
   public class TipoDocumentoDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debe especificar si es requerido")]
        public bool EsRequerido { get; set; }

        [Required(ErrorMessage = "El orden es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor a 0")]
        public int? Orden { get; set; }
    }
}
