namespace ProyectoNomina.Backend.Models
{
    public class TipoDocumento
    {
        public int Id { get; set; }

        // Inicializado para evitar CS8618 y mantener no-nullable
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        // Valor por defecto útil hoy y a futuro
        public bool EsRequerido { get; set; } = true;

        public int? Orden { get; set; }

        // Navegación: lista inicializada para evitar nulls
        public ICollection<DocumentoEmpleado> DocumentosEmpleados { get; set; } = new List<DocumentoEmpleado>();
    }
}
