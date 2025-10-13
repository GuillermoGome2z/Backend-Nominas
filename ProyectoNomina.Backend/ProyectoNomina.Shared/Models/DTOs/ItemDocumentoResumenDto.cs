using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNomina.Shared.Models.DTOs
{
    public class ItemDocumentoResumenDto
    {
        public string Tipo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
