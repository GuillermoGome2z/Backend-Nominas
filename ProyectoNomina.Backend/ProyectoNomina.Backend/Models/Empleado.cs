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

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = null!;

        [Required(ErrorMessage = "El salario mensual es obligatorio")]
        [Precision(18, 2)]
        public decimal SalarioMensual { get; set; }

        public DateTime FechaContratacion { get; set; }

        [Required(ErrorMessage = "El DPI es obligatorio")]
        public string DPI { get; set; } = null!;

        [Required(ErrorMessage = "El NIT es obligatorio")]
        public string NIT { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        public DateTime FechaNacimiento { get; set; }

        public string EstadoLaboral { get; set; } = "Activo";

        // Relaciones foráneas
        public int? DepartamentoId { get; set; }

        [JsonIgnore]
        public Departamento? Departamento { get; set; }

        public int PuestoId { get; set; }

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
