using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    /// <summary>
    /// DTO para mostrar información completa del empleado
    /// Usado en respuestas GET y operaciones de lectura
    /// </summary>
    public class EmpleadoDto
    {
        public int Id { get; set; }
        
        public string NombreCompleto { get; set; } = string.Empty;
        
        public string? DPI { get; set; }
        
        public string? NIT { get; set; }
        
        public string? Correo { get; set; }
        
        public string? Telefono { get; set; }
        
        public string? Direccion { get; set; }
        
        public DateTime? FechaNacimiento { get; set; }
        
        public DateTime FechaContratacion { get; set; }
        
        /// <summary>
        /// Estado laboral del empleado: ACTIVO, SUSPENDIDO, RETIRADO
        /// </summary>
        public string EstadoLaboral { get; set; } = "ACTIVO";
        
        public decimal SalarioMensual { get; set; }
        
        public int? DepartamentoId { get; set; }
        
        public string? NombreDepartamento { get; set; }
        
        public int? PuestoId { get; set; }
        
        public string? NombrePuesto { get; set; }
    }
}
