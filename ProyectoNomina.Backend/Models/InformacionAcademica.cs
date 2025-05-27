using System;

namespace ProyectoNomina.Backend.Models
{
    public class InformacionAcademica
    {
        public int Id { get; set; }

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; }

        public string Titulo { get; set; } = null!;
        public string Institucion { get; set; } = null!;
        public DateTime FechaGraduacion { get; set; }
        public string TipoCertificacion { get; set; } = null!;
    }
}
