using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class UsuarioRegistroDto
    {
        [Required] public required string Nombre { get; init; }
        [Required, EmailAddress] public required string Correo { get; init; }
        [Required] public required string Contraseña { get; init; }

        [Required] public string Rol { get; init; } = "Usuario";
        public int? EmpleadoId { get; init; }
    }
}