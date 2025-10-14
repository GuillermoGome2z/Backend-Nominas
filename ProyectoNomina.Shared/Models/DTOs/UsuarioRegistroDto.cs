using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class UsuarioRegistroDto
    {
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Correo { get; set; }
        [Required]
        public string Contraseña { get; set; }
        [Required]
        public string Rol { get; set; } = "Usuario";

        public int? EmpleadoId { get; set; }
    }
}
