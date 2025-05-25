namespace ProyectoNomina.Backend.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public string Accion { get; set; }
        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }
        public string Detalles { get; set; }
    }
}

