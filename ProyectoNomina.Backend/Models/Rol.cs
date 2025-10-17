using System.Text.Json.Serialization;
using ProyectoNomina.Shared.Models.DTOs;
namespace ProyectoNomina.Backend.Models;

    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;


        [JsonIgnore] //  Evita ciclos infinitos o errores de serialización
        public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }


