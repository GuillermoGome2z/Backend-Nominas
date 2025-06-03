using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class RegistrarUsuarioDto
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Clave  { get; set; } = string.Empty;
        public int RolId { get; set; }
    }
}
