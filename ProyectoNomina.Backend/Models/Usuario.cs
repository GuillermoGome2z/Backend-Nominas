using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProyectoNomina.Shared.Models.DTOs;
namespace ProyectoNomina.Backend.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        [Required]
        public string NombreCompleto { get; set; }
        [Required]
        public string Correo { get; set; }
        [Required]
        public string ClaveHash { get; set; } 
        [Required]
        public string Rol { get; set; } = "Usuario";

        public int? EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }
    }
}
