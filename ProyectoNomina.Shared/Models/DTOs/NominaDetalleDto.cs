namespace ProyectoNomina.Shared.Models.DTOs
{
    public class NominaDetalleDto
    {
        public int NominaId { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public decimal TotalBruto { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal TotalBonificaciones { get; set; }
        public decimal TotalNeto { get; set; }

        public List<DetalleNominaDto> Items { get; set; } = new();
    }
}
