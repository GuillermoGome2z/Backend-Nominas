using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,RRHH")] // 🔐 Acceso solo para roles específicos
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 🧾 Retorna un resumen de todas las nóminas procesadas con totales por categoría.
        /// </summary>
        [HttpGet("Nominas")]
        public async Task<ActionResult<IEnumerable<ReporteNominaDto>>> ObtenerReporteNominas()
        {
            var nominas = await _context.Nominas
                .Include(n => n.Detalles)
                .ToListAsync();

            if (nominas == null || nominas.Count == 0)
                return NotFound("No se encontraron nóminas registradas.");

            var reporte = nominas.Select(n => new ReporteNominaDto
            {
                NominaId = n.Id,
                Descripcion = n.Descripcion,
                FechaGeneracion = n.FechaGeneracion,
                TotalSalarios = n.Detalles.Sum(d => d.SalarioBruto),
                TotalBonificaciones = n.Detalles.Sum(d => d.Bonificaciones),
                TotalDeducciones = n.Detalles.Sum(d => d.Deducciones),
                TotalNeto = n.Detalles.Sum(d => d.SalarioNeto)
            }).ToList();

            return Ok(reporte);
        }

        /// <summary>
        /// 📄 Retorna el estado del expediente para todos los empleados con base en los documentos requeridos.
        /// </summary>
        [HttpGet("Expedientes")]
        public async Task<ActionResult<IEnumerable<ReporteExpedienteDto>>> ObtenerReporteExpedientes()
        {
            // Obtener todos los empleados
            var empleados = await _context.Empleados.ToListAsync();

            // Obtener los tipos de documento requeridos
            var tiposRequeridos = await _context.TiposDocumento
                .Where(t => t.EsRequerido)
                .Select(t => t.Id)
                .ToListAsync();

            // Generar el reporte por cada empleado
            var reporte = new List<ReporteExpedienteDto>();

            foreach (var emp in empleados)
            {
                var entregados = await _context.DocumentosEmpleado
                    .Where(d => d.EmpleadoId == emp.Id)
                    .Select(d => d.TipoDocumentoId)
                    .Distinct()
                    .ToListAsync();

                var faltantes = tiposRequeridos.Except(entregados).ToList();

                string estado = faltantes.Count == 0 ? "Completo"
                               : entregados.Count == 0 ? "Incompleto"
                               : "En proceso";

                reporte.Add(new ReporteExpedienteDto
                {
                    Empleado = emp.NombreCompleto,
                    EstadoExpediente = estado,
                    DocumentosRequeridos = tiposRequeridos.Count,
                    DocumentosPresentados = entregados.Count,
                    DocumentosFaltantes = faltantes.Count
                });
            }

            return Ok(reporte);
        }
    }
}

