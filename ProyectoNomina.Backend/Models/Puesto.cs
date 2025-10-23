using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace ProyectoNomina.Backend.Models
{
    public class Puesto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal SalarioBase { get; set; }

        /// <summary>Controla si el puesto está disponible para asignar a empleados.</summary>
        public bool Activo { get; set; } = true;

        // FK a Departamento (puede ser nullable si tienes datos sin departamento)
        public int? DepartamentoId { get; set; }
        public Departamento? Departamento { get; set; }

        [JsonIgnore]
        public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    }
}
