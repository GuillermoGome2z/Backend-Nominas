using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class DepartamentoDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre del departamento es obligatorio")]
        public string Nombre { get; set; } = string.Empty;
    }
}
