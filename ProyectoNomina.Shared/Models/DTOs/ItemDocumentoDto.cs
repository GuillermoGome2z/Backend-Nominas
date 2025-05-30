using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class ItemDocumentoDto
    {
        public string Tipo { get; set; } = string.Empty;
        public int TotalRequeridos { get; set; }
        public int Entregados { get; set; }
        public int Faltantes { get; set; }
    }
}
