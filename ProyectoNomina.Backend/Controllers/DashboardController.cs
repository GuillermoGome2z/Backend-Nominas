using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,RRHH")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/dashboard
       [HttpGet]
public async Task<ActionResult<DashboardKpisDto>> GetKpis()
{
    // Total empleados
    var totalEmpleados = await _context.Empleados.CountAsync();

    // Activos por departamento (null-safe)
    var activosPorDepto = await _context.Empleados
        .GroupBy(e => new 
        { 
            e.DepartamentoId, 
            Nombre = e.Departamento != null ? e.Departamento.Nombre : "Sin departamento" 
        })
        .Select(g => new ActivosPorDepartamentoDto
        {
            Departamento = g.Key.Nombre,
            Activos = g.Count()
        })
        .OrderByDescending(x => x.Activos)
        .ToListAsync();

    // Fechas en TZ Guatemala
    var tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); // Windows name para GT
    var ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    var inicioMesActualLocal = new DateTime(ahoraLocal.Year, ahoraLocal.Month, 1);
    var inicioMesAnteriorLocal = inicioMesActualLocal.AddMonths(-1);

    // Cantidad de nóminas del último mes (en base a Nomina.FechaGeneracion)
    var nominasUltimoMesCount = await _context.Nominas
        .Where(n => n.FechaGeneracion >= inicioMesAnteriorLocal && n.FechaGeneracion < inicioMesActualLocal)
        .CountAsync();

    // Total neto del último mes (join explícito evita navegar propiedades nulas)
    var nominasUltimoMesTotal = await _context.DetalleNominas
        .Where(d => _context.Nominas
            .Where(n => n.FechaGeneracion >= inicioMesAnteriorLocal && n.FechaGeneracion < inicioMesActualLocal)
            .Select(n => n.Id).Contains(d.NominaId))
        .SumAsync(d => (decimal?)d.SalarioNeto) ?? 0m;

    return Ok(new DashboardKpisDto
    {
        TotalEmpleados = totalEmpleados,
        ActivosPorDepartamento = activosPorDepto,
        NominasUltimoMesCount = nominasUltimoMesCount,
        NominasUltimoMesTotal = nominasUltimoMesTotal
    });
}

    }
}
