namespace ProyectoNomina.Backend.Models
{
    public class TipoDocumento
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public ICollection<DocumentoEmpleado> Documentos { get; set; }
    }
}
