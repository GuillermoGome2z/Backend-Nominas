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
    public class DocumentosEmpleadoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentosEmpleadoController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadDocumento([FromForm] DocumentoSubidaDto dto)
        {
            if (dto.Archivo == null || dto.Archivo.Length == 0)
                return BadRequest("Archivo no válido");

            try
            {
                var nombreArchivo = $"{Guid.NewGuid()}_{Path.GetFileName(dto.Archivo.FileName)}";
                var rutaCarpeta = Path.Combine(_env.WebRootPath, "documentos");

                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await dto.Archivo.CopyToAsync(stream);
                }

                var documento = new DocumentoEmpleado
                {
                    EmpleadoId = dto.EmpleadoId,
                    TipoDocumentoId = dto.TipoDocumentoId,
                    RutaArchivo = $"documentos/{nombreArchivo}",
                    FechaSubida = DateTime.Now
                };

                _context.DocumentosEmpleado.Add(documento);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "✅ Archivo subido correctamente.",
                    documento.Id,
                    documento.RutaArchivo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // GET: api/DocumentosEmpleado
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentoEmpleadoDto>>> GetDocumentos()
        {
            var documentos = await _context.DocumentosEmpleado
                .Include(d => d.Empleado)
                .Include(d => d.TipoDocumento)
                .Select(d => new DocumentoEmpleadoDto
                {
                    Id = d.Id,
                    EmpleadoId = d.EmpleadoId,
                    NombreEmpleado = d.Empleado.NombreCompleto,
                    TipoDocumentoId = d.TipoDocumentoId,
                    NombreTipo = d.TipoDocumento.Nombre,
                    RutaArchivo = d.RutaArchivo,
                    FechaSubida = d.FechaSubida
                })
                .ToListAsync();

            return documentos;
        }

        // GET: api/DocumentosEmpleado/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentoEmpleado>> GetDocumento(int id)
        {
            var doc = await _context.DocumentosEmpleado
                .Include(d => d.Empleado)
                .Include(d => d.TipoDocumento)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null)
                return NotFound();

            return doc;
        }

        // POST sin archivo (por si se desea usar manualmente)
        [HttpPost]
        public async Task<ActionResult<DocumentoEmpleado>> PostDocumento(DocumentoEmpleado doc)
        {
            doc.FechaSubida = DateTime.Now;
            _context.DocumentosEmpleado.Add(doc);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDocumento), new { id = doc.Id }, doc);
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocumento(int id, DocumentoEmpleado doc)
        {
            if (id != doc.Id)
                return BadRequest();

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

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocumento(int id)
        {
            var doc = await _context.DocumentosEmpleado.FindAsync(id);
            if (doc == null)
                return NotFound();

            _context.DocumentosEmpleado.Remove(doc);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
