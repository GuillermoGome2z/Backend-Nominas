using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    /// <summary>
    /// DTO para cambiar el estado laboral de un empleado
    /// Usado en PUT /api/Empleados/{id}/estado
    /// </summary>
    public class CambiarEstadoEmpleadoDto
    {
        [Required(ErrorMessage = "El estado laboral es obligatorio")]
        [RegularExpression(@"^(ACTIVO|SUSPENDIDO|RETIRADO)$", ErrorMessage = "El estado laboral debe ser: ACTIVO, SUSPENDIDO o RETIRADO")]
        public string EstadoLaboral { get; set; } = string.Empty;
    }
}