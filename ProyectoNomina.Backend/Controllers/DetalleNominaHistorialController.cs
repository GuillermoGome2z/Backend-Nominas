using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/DetalleNominas")]
    public class DetalleNominaHistorialController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DetalleNominaHistorialController(AppDbContext context) => _context = context;

        // GET /api/DetalleNominas/{id}/historial
        [HttpGet("{id:int}/historial")]
        public async Task<IActionResult> GetHistorial(int id, CancellationToken ct = default)
        {
            var existe = await _context.DetalleNominas.AsNoTracking().AnyAsync(d => d.Id == id, ct);
            if (!existe) return NotFound($"No existe DetalleNomina con id {id}.");

            var data = await _context.Set<ProyectoNomina.Backend.Models.DetalleNominaHistorial>()
                .AsNoTracking()
                .Where(h => h.DetalleNominaId == id)
                .OrderByDescending(h => h.Fecha)
                .Select(h => new {
                    h.Id,
                    h.DetalleNominaId,
                    h.Campo,
                    h.ValorAnterior,
                    h.ValorNuevo,
                    h.UsuarioId,
                    h.Fecha
                })
                .ToListAsync(ct);

            return Ok(data);
        }
    }
}
