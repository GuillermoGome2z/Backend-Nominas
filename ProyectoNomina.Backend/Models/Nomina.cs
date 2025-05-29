using ProyectoNomina.Shared.Models.DTOs;
namespace ProyectoNomina.Backend.Models
{
    public class Nomina
    {
        public int Id { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Descripcion { get; set; }

        public ICollection<DetalleNomina> Detalles { get; set; }
    }
}
