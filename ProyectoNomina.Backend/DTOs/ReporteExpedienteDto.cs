namespace ProyectoNomina.Backend.DTOs
{
    public class ReporteExpedienteDto
    {
        public string Empleado { get; set; }
        public string EstadoExpediente { get; set; }
        public int DocumentosRequeridos { get; set; }
        public int DocumentosPresentados { get; set; }
        public int DocumentosFaltantes { get; set; }
    }
}

