using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class PuestoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del puesto es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El salario base debe ser >= 0")]
        public decimal SalarioBase { get; set; }

        /// <summary>Activo/inactivo.</summary>
        public bool Activo { get; set; } = true;

        /// <summary>Departamento al que pertenece el puesto.</summary>
        public int? DepartamentoId { get; set; }
    }
}
