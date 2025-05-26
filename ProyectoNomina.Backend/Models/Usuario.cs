using System.Text.Json.Serialization;

namespace ProyectoNomina.Backend.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string ClaveHash { get; set; }

        [JsonIgnore]
        public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }
}
