using System.Text.Json.Serialization;

namespace ProyectoNomina.Backend.Models
{
    public class Departamento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Controla si el departamento está disponible para asignar a empleados.</summary>
        public bool Activo { get; set; } = true;

        [JsonIgnore]
        public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

        // Relación 1..N con Puestos
        [JsonIgnore]
        public ICollection<Puesto> Puestos { get; set; } = new List<Puesto>();
    }
}
