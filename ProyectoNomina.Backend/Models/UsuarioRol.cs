using System.Text.Json.Serialization;
using ProyectoNomina.Shared.Models.DTOs;
namespace ProyectoNomina.Backend.Models
{
    public class UsuarioRol
    {
        public int UsuarioId { get; set; }

        [JsonIgnore] // ✅ Esto rompe el ciclo entre Usuario <-> UsuarioRoles
        public Usuario? Usuario { get; set; }

        public int RolId { get; set; }

        [JsonIgnore] // ✅ Esto rompe el ciclo entre Rol <-> UsuarioRoles
        public Rol? Rol { get; set; }
    }
}

