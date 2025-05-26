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
    public class DeduccionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeduccionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Deducciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Deduccion>>> GetDeducciones()
        {
            return await _context.Deducciones.ToListAsync();
        }

        // GET: api/Deducciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Deduccion>> GetDeduccion(int id)
        {
            var deduccion = await _context.Deducciones.FindAsync(id);
            if (deduccion == null) return NotFound();
            return deduccion;
        }

        // POST: api/Deducciones
        [HttpPost]
        public async Task<ActionResult<Deduccion>> PostDeduccion(Deduccion deduccion)
        {
            _context.Deducciones.Add(deduccion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeduccion), new { id = deduccion.Id }, deduccion);
        }

        // PUT: api/Deducciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDeduccion(int id, Deduccion deduccion)
        {
            if (id != deduccion.Id) return BadRequest();

            _context.Entry(deduccion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Deducciones.Any(d => d.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Deducciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeduccion(int id)
        {
            var deduccion = await _context.Deducciones.FindAsync(id);
            if (deduccion == null) return NotFound();

            _context.Deducciones.Remove(deduccion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

