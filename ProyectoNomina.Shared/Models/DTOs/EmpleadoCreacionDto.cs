using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public sealed class EmpleadoCreacionDto
    {
        [Required] public required string NombreCompleto { get; init; }
        [Required, EmailAddress] public required string Correo { get; init; }
        [Required] public required string Telefono { get; init; }
        [Required] public required string Direccion { get; init; }

        [Required, RegularExpression(@"^\d{13}$")]
        public required string DPI { get; init; }

        [Required, RegularExpression(@"^[0-9A-Za-z-]{7,15}$")]
        public required string NIT { get; init; }

        [Required, DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; init; }

        //  Campos que tu EmpleadosController está leyendo
        [Required, Range(1, int.MaxValue)]
        public int DepartamentoId { get; init; }

        [Required, Range(1, int.MaxValue)]
        public int PuestoId { get; init; }

        // Si el salario lo toma del Puesto en BD, esto podría ser opcional.
        // Como tu controller lo usa (línea 186), lo exponemos y validamos.
        [Required, Range(0, double.MaxValue)]
        public decimal SalarioBase { get; init; }

        [Required, DataType(DataType.Date)]
        public DateTime FechaContratacion { get; init; }

        // Opcional, con default controlado
        public string EstadoLaboral { get; init; } = "Activo";
    }
}
