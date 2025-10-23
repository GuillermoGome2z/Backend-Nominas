using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    /// <summary>
    /// DTO para crear y actualizar empleados
    /// Usado en operaciones POST y PUT
    /// </summary>
    public class EmpleadoCreateUpdateDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre completo no puede exceder 200 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [StringLength(13, ErrorMessage = "El DPI debe tener 13 dígitos")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "El DPI debe contener exactamente 13 dígitos")]
        public string? DPI { get; set; }
        
        [StringLength(15, ErrorMessage = "El NIT no puede exceder 15 caracteres")]
        [RegularExpression(@"^[0-9A-Za-z-]{7,15}$", ErrorMessage = "El NIT debe tener entre 7 y 15 caracteres alfanuméricos")]
        public string? NIT { get; set; }
        
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        [StringLength(256, ErrorMessage = "El correo no puede exceder 256 caracteres")]
        public string? Correo { get; set; }
        
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Telefono { get; set; }
        
        [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
        public string? Direccion { get; set; }
        
        public DateTime? FechaNacimiento { get; set; }
        
        [Required(ErrorMessage = "La fecha de contratación es obligatoria")]
        public DateTime FechaContratacion { get; set; }
        
        [Required(ErrorMessage = "El salario mensual es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El salario mensual debe ser mayor o igual a 0")]
        public decimal SalarioMensual { get; set; }
        
        public int? DepartamentoId { get; set; }
        
        public int? PuestoId { get; set; }
    }
}