using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartamentosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Departamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Departamento>>> GetDepartamentos()
        {
            return await _context.Departamentos.ToListAsync();
        }

        // GET: api/Departamentos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Departamento>> GetDepartamento(int id)
        {
            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento == null) return NotFound();
            return departamento;
        }

        // POST: api/Departamentos
        [HttpPost]
        public async Task<ActionResult<Departamento>> PostDepartamento(Departamento departamento)
        {
            _context.Departamentos.Add(departamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartamento), new { id = departamento.Id }, departamento);
        }

        // PUT: api/Departamentos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartamento(int id, Departamento departamento)
        {
            if (id != departamento.Id) return BadRequest();

            _context.Entry(departamento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Departamentos.Any(d => d.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Departamentos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartamento(int id)
        {
            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento == null) return NotFound();

            _context.Departamentos.Remove(departamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
