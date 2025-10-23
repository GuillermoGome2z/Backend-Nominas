using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Backend.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string Correo { get; set; } = string.Empty;

        //  Hash requerido (nunca texto plano)
        [Required]
        public string ClaveHash { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Rol { get; set; } = "Usuario";

        public int? EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }

        //  Navegación 1:N a RefreshTokens (para la relación fluida)
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
