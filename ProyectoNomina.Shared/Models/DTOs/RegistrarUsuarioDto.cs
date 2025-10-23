using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class RegistrarUsuarioDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZÁÉÍÓÚáéíóúñÑ\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Contraseña { get; set; } = string.Empty;

        public int? EmpleadoId { get; set; }
        public string Rol { get; set; } = "Usuario";
    }
}
