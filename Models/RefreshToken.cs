using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Backend.Models
{
    /// <summary>
    /// Token de renovación para autenticación JWT
    /// </summary>
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

        /// <summary>
        /// Fecha y hora de creación del token
        /// </summary>
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora cuando el token fue renovado o revocado
        /// </summary>
        public DateTime? RenovadoEn { get; set; }

        // Navegación a Usuario para EF y consultas
        public Usuario Usuario { get; set; } = null!;
    }
}
