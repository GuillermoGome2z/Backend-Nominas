using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Backend.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required, MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime Expira { get; set; }

        public bool Revocado { get; set; } = false;

        // Navegación a Usuario para EF y consultas
        public Usuario Usuario { get; set; } = null!;
    }
}
