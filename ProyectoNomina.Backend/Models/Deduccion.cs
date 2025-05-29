using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Models
{
    public class Deduccion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        [Precision(18, 2)]
        public decimal Monto { get; set; }
    }
}