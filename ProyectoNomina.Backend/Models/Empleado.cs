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
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        public string EstadoLaboral { get; set; } = "Activo";

        public int Id { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Correo { get; set; }

        [Required]
        public string NombreCompleto { get; set; } = null!;

        [Required]
        public string DPI { get; set; } = null!;

        [Required]
        public string NIT { get; set; } = null!;

        [Required]
        public string Direccion { get; set; } = null!;

        [Required]
        public string Telefono { get; set; } = null!;

        public DateTime FechaContratacion { get; set; }

        [Precision(18, 2)]
        public decimal SalarioMensual { get; set; }

        // Relaciones con clave foránea
        public int? DepartamentoId { get; set; }

        [JsonIgnore]
        public Departamento? Departamento { get; set; }

        public int PuestoId { get; set; }

        [JsonIgnore]
        public Puesto? Puesto { get; set; }

        // Relaciones de colección
        [JsonIgnore]
        public ICollection<DocumentoEmpleado>? Documentos { get; set; }

        [JsonIgnore]
        public ICollection<DetalleNomina>? DetallesNomina { get; set; }

        [JsonIgnore]
        public ICollection<InformacionAcademica>? Estudios { get; set; }
    }
}
