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
            // 1️⃣ Total de empleados
            var totalEmpleados = await _context.Empleados.CountAsync();

            // 2️⃣ Activos por departamento
            // Si tu modelo Empleado no tiene un booleano "Activo",
            // puedes filtrar por FechaSalida == null o Estatus == "Activo"
            var activosPorDepto = await _context.Empleados
                //.Where(e => e.FechaSalida == null) // Descomenta si ese es tu criterio
                .GroupBy(e => new { e.DepartamentoId, Nombre = e.Departamento.Nombre })
                .Select(g => new ActivosPorDepartamentoDto
                {
                    Departamento = g.Key.Nombre,
                    Activos = g.Count()
                })
                .OrderByDescending(x => x.Activos)
                .ToListAsync();

            // 3️⃣ Nóminas del último mes (usando FechaGeneracion de tu modelo)
            var hoy = DateTime.UtcNow;
            var inicioMesActual = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesAnterior = inicioMesActual.AddMonths(-1);

            // Cantidad de nóminas generadas
            var nominasUltimoMesCount = await _context.Nominas
                .Where(n => n.FechaGeneracion >= inicioMesAnterior && n.FechaGeneracion < inicioMesActual)
                .CountAsync();

            // Total del último mes (suma de SalarioNeto desde los detalles)
            var nominasUltimoMesTotal = await _context.DetalleNominas
                .Where(d => d.Nomina.FechaGeneracion >= inicioMesAnterior && d.Nomina.FechaGeneracion < inicioMesActual)
                .SumAsync(d => (decimal?)d.SalarioNeto) ?? 0m;

            // DTO de respuesta
            var dto = new DashboardKpisDto
            {
                TotalEmpleados = totalEmpleados,
                ActivosPorDepartamento = activosPorDepto,
                NominasUltimoMesCount = nominasUltimoMesCount,
                NominasUltimoMesTotal = nominasUltimoMesTotal
            };

            return Ok(dto);
        }
    }
}
