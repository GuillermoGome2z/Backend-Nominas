using System;

namespace ProyectoNomina.Backend.Models
{
    public class AuditoriaLog
    {
        public int Id { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string Accion { get; set; } = string.Empty;

        public string Detalles { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
    }
}
