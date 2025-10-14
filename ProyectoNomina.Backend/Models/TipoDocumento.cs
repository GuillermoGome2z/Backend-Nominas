using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;
public class TipoDocumento
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public bool EsRequerido { get; set; } = true;
    public int? Orden { get; set; }

    public ICollection<DocumentoEmpleado> DocumentosEmpleados { get; set; } = new List<DocumentoEmpleado>();

}
