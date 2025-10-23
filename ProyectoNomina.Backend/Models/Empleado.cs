using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ProyectoNomina.Backend.Models
{
    [Index(nameof(NIT), IsUnique = true)]
    [Index(nameof(DPI), IsUnique = true)]
    [Index(nameof(Correo), IsUnique = true)]
    public class Empleado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string NombreCompleto { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public string? Direccion { get; set; }

        [Required(ErrorMessage = "El salario mensual es obligatorio")]
        [Precision(18, 2)]
        public decimal SalarioMensual { get; set; }

        public DateTime FechaContratacion { get; set; }

        public string? DPI { get; set; }

        public string? NIT { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public string EstadoLaboral { get; set; } = "ACTIVO";

        // Relaciones foráneas
        public int? DepartamentoId { get; set; }

        [JsonIgnore]
        public Departamento? Departamento { get; set; }

        public int? PuestoId { get; set; }

        [JsonIgnore]
        public Puesto? Puesto { get; set; }

        // Relaciones 1:N
        [JsonIgnore]
        public ICollection<DocumentoEmpleado>? Documentos { get; set; }

        [JsonIgnore]
        public ICollection<DetalleNomina>? DetallesNomina { get; set; }

        [JsonIgnore]
        public ICollection<InformacionAcademica>? Estudios { get; set; }

        [JsonIgnore]
        public ICollection<Bonificacion>? Bonificaciones { get; set; }

        [JsonIgnore]
        public ICollection<Deduccion>? Deducciones { get; set; }

        // Relación 1:1 con Usuario
        public Usuario? Usuario { get; set; }
    }
}
