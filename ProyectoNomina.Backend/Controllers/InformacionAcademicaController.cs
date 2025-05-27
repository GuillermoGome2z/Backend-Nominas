using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,RRHH")]
    public class InformacionAcademicaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InformacionAcademicaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/InformacionAcademica
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InformacionAcademica>>> GetInformacionAcademica()
        {
            return await _context.InformacionAcademica.Include(i => i.Empleado).ToListAsync();
        }

        // GET: api/InformacionAcademica/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InformacionAcademica>> GetInformacion(int id)
        {
            var info = await _context.InformacionAcademica.FindAsync(id);

            if (info == null)
                return NotFound();

            return info;
        }

        // POST: api/InformacionAcademica
        [HttpPost]
        public async Task<ActionResult<InformacionAcademica>> PostInformacion(InformacionAcademica info)
        {
            _context.InformacionAcademica.Add(info);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInformacion), new { id = info.Id }, info);
        }

        // PUT: api/InformacionAcademica/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInformacion(int id, InformacionAcademica info)
        {
            if (id != info.Id)
                return BadRequest();

            _context.Entry(info).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.InformacionAcademica.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/InformacionAcademica/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInformacion(int id)
        {
            var info = await _context.InformacionAcademica.FindAsync(id);
            if (info == null)
                return NotFound();

            _context.InformacionAcademica.Remove(info);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
