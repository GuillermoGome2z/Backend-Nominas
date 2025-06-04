using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class EmpleadoDto
    {
        public int Id { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El número de teléfono es incorrecto")]
        public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El salario es obligatorio")]
    public decimal SalarioMensual { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un departamento")]
    public int DepartamentoId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un puesto")]
    public int PuestoId { get; set; }

    [Required(ErrorMessage = "La fecha de contratación es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaContratacion { get; set; }

    [Required(ErrorMessage = "El DPI es obligatorio")]
    public string DPI { get; set; } = string.Empty;

    [Required(ErrorMessage = "El NIT es obligatorio")]
    public string NIT { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaNacimiento { get; set; }

    public string EstadoLaboral { get; set; } = "Activo";

    // Solo para visualización
    public string? NombreDepartamento { get; set; }
    public string? NombrePuesto { get; set; }
}
}
