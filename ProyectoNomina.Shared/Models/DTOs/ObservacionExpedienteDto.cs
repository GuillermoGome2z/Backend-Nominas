namespace ProyectoNomina.Shared.Models.DTOs
{
    public class ObservacionExpedienteDto
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public int? DocumentoEmpleadoId { get; set; }
        public string Texto { get; set; } = string.Empty;

        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
