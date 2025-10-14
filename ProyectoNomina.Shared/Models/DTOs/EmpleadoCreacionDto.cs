using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class EmpleadoCreacionDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Correo { get; set; }


        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El número de teléfono es incorrecto")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El salario base es obligatorio")]
        public decimal SalarioBase { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un departamento")]
        public int DepartamentoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un puesto")]
        public int PuestoId { get; set; }

        [Required(ErrorMessage = "La fecha de contratación es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaContratacion { get; set; }

        [Required(ErrorMessage = "El DPI es obligatorio")]
        public string DPI { get; set; }

        [Required(ErrorMessage = "El NIT es obligatorio")]
        public string NIT { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        public string EstadoLaboral { get; set; } = "Activo";
    }
}
