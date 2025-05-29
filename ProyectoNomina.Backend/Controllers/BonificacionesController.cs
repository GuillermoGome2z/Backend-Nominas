using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;


namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/[controller]")]
    public class BonificacionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BonificacionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Bonificaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bonificacion>>> GetBonificaciones()
        {
            return await _context.Bonificaciones.ToListAsync();
        }

        // GET: api/Bonificaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bonificacion>> GetBonificacion(int id)
        {
            var bono = await _context.Bonificaciones.FindAsync(id);
            if (bono == null) return NotFound();
            return bono;
        }

        // POST: api/Bonificaciones
        [HttpPost]
        public async Task<ActionResult<Bonificacion>> PostBonificacion(Bonificacion bono)
        {
            _context.Bonificaciones.Add(bono);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBonificacion), new { id = bono.Id }, bono);
        }

        // PUT: api/Bonificaciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBonificacion(int id, Bonificacion bono)
        {
            if (id != bono.Id) return BadRequest();

            _context.Entry(bono).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Bonificaciones.Any(b => b.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Bonificaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBonificacion(int id)
        {
            var bono = await _context.Bonificaciones.FindAsync(id);
            if (bono == null) return NotFound();

            _context.Bonificaciones.Remove(bono);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
