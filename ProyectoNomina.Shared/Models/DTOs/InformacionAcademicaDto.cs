using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class InformacionAcademicaDto
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Institucion { get; set; } = string.Empty;
        public DateTime FechaGraduacion { get; set; }
    }
}
