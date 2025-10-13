using ProyectoNomina.Backend.Models;
using System.Text.Json.Serialization;
using ProyectoNomina.Shared.Models.DTOs;

public class Departamento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
