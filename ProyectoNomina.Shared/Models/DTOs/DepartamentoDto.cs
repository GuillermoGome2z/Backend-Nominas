using System.ComponentModel.DataAnnotations;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class DepartamentoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del departamento es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Si no envías este campo en PUT, el controlador puede ignorarlo o mantener su valor.
        /// </summary>
        public bool Activo { get; set; } = true;
    }
}
