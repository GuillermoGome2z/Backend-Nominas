using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize] // ⬅️ protege todo el controlador
    [ApiController]
    [Route("api/[controller]")]
    public class DetalleNominasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetalleNominasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DetalleNominas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleNomina>>> GetDetalles()
        {
            return await _context.DetalleNominas
                .Include(d => d.Empleado)
                .Include(d => d.Nomina)
                .ToListAsync();
        }

        // GET: api/DetalleNominas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleNomina>> GetDetalle(int id)
        {
            var detalle = await _context.DetalleNominas
                .Include(d => d.Empleado)
                .Include(d => d.Nomina)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (detalle == null) return NotFound();
            return detalle;
        }

        // POST: api/DetalleNominas
        [HttpPost]
        public async Task<ActionResult<DetalleNomina>> PostDetalle(DetalleNomina detalle)
        {
            // Cálculo del salario neto
            detalle.SalarioNeto = detalle.SalarioBruto + detalle.Bonificaciones - detalle.Deducciones;

            _context.DetalleNominas.Add(detalle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetalle), new { id = detalle.Id }, detalle);
        }

        // PUT: api/DetalleNominas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalle(int id, DetalleNomina detalle)
        {
            if (id != detalle.Id) return BadRequest();

            detalle.SalarioNeto = detalle.SalarioBruto + detalle.Bonificaciones - detalle.Deducciones;

            _context.Entry(detalle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DetalleNominas.Any(d => d.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/DetalleNominas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalle(int id)
        {
            var detalle = await _context.DetalleNominas.FindAsync(id);
            if (detalle == null) return NotFound();

            _context.DetalleNominas.Remove(detalle);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
