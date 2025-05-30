using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Models
{
    public class Empleado
    {
        public DateTime FechaNacimiento { get; set; }
        public string EstadoLaboral { get; set; } = "Activo";
        public int Id { get; set; }
        public string Correo { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string DPI { get; set; } = null!;
        public string NIT { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public DateTime FechaContratacion { get; set; }
        [Precision(18, 2)]
        public decimal SalarioMensual { get; set; }

        // Relaciones con clave foránea
        public int DepartamentoId { get; set; }

        [JsonIgnore] // No se requiere en POST ni PUT
        public Departamento? Departamento { get; set; }

        public int PuestoId { get; set; }

        [JsonIgnore] // No se requiere en POST ni PUT
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