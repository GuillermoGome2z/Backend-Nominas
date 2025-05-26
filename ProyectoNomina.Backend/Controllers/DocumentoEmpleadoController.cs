using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentosEmpleadoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DocumentosEmpleadoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DocumentosEmpleado
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentoEmpleado>>> GetDocumentos()
        {
            return await _context.DocumentosEmpleado
                .Include(d => d.Empleado)
                .Include(d => d.TipoDocumento)
                .ToListAsync();
        }

        // GET: api/DocumentosEmpleado/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentoEmpleado>> GetDocumento(int id)
        {
            var doc = await _context.DocumentosEmpleado
                .Include(d => d.Empleado)
                .Include(d => d.TipoDocumento)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return NotFound();
            return doc;
        }

        // POST: api/DocumentosEmpleado
        [HttpPost]
        public async Task<ActionResult<DocumentoEmpleado>> PostDocumento(DocumentoEmpleado doc)
        {
            doc.FechaSubida = DateTime.Now; // Registrar la fecha actual
            _context.DocumentosEmpleado.Add(doc);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDocumento), new { id = doc.Id }, doc);
        }

        // PUT: api/DocumentosEmpleado/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocumento(int id, DocumentoEmpleado doc)
        {
            if (id != doc.Id) return BadRequest();

            _context.Entry(doc).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DocumentosEmpleado.Any(d => d.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/DocumentosEmpleado/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocumento(int id)
        {
            var doc = await _context.DocumentosEmpleado.FindAsync(id);
            if (doc == null) return NotFound();

            _context.DocumentosEmpleado.Remove(doc);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
