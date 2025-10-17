using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace ProyectoNomina.Shared.Models.DTOs
{
    public class ReporteNominaDto
{
    public int NominaId { get; init; }
    [Required, MinLength(3)] public required string Descripcion { get; init; }
    public DateTime FechaGeneracion { get; init; }
    public decimal TotalSalarios { get; init; }
    public decimal TotalBonificaciones { get; init; }
    public decimal TotalDeducciones { get; init; }
    public decimal TotalNeto { get; init; }
}
}
