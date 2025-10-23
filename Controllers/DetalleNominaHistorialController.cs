using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
// 👉 Agregado:
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/DetalleNominasHistorial")]
    public class DetalleNominaHistorialController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DetalleNominaHistorialController(AppDbContext context) => _context = context;

        // GET /api/DetalleNominasHistorial/{detalleNominaId}
        [HttpGet("{detalleNominaId:int}")]
        public async Task<IActionResult> GetHistorial(int detalleNominaId, CancellationToken ct = default)
        {
            var existe = await _context.DetalleNominas
                .AsNoTracking()
                .AnyAsync(d => d.Id == detalleNominaId, ct);
            if (!existe) return NotFound($"No existe DetalleNomina con id {detalleNominaId}.");

            var data = await _context.Set<DetalleNominaHistorial>()
                .AsNoTracking()
                .Where(h => h.DetalleNominaId == detalleNominaId)
                .OrderByDescending(h => h.Fecha)
                .Select(h => new
                {
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
