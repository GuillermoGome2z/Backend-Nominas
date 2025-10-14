using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;
using System.Security.Claims;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/DocumentosEmpleado")] // 🔁 Forzamos la ruta compatible con el frontend
    // Por tener [Authorize], documentamos 401 y 403 a nivel de clase
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class DocumentoEmpleadoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentoEmpleadoController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // POST: Subir documento con archivo
        [HttpPost("Upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        [ProducesResponseType(typeof(IEnumerable<DocumentoEmpleadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        [ProducesResponseType(typeof(DocumentoEmpleadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DocumentoEmpleadoDto>> GetDocumento(int id)
        {
            var doc = await _context.DocumentosEmpleado
                .Include(d => d.Empleado)
                .Include(d => d.TipoDocumento)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null)
                return NotFound();

            return new DocumentoEmpleadoDto
            {
                Id = doc.Id,
                EmpleadoId = doc.EmpleadoId,
                TipoDocumentoId = doc.TipoDocumentoId,
                RutaArchivo = doc.RutaArchivo,
                FechaSubida = doc.FechaSubida,
                NombreEmpleado = doc.Empleado.NombreCompleto,
                NombreTipo = doc.TipoDocumento.Nombre
            };
        }

        // PUT: Actualizar tipo de documento y/o archivo
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarDocumento(int id)
        {
            var documento = await _context.DocumentosEmpleado.FindAsync(id);
            if (documento == null)
                return NotFound();

            var form = await Request.ReadFormAsync();

            if (form.TryGetValue("TipoDocumentoId", out var tipoIdValue) &&
                int.TryParse(tipoIdValue, out var tipoId))
            {
                documento.TipoDocumentoId = tipoId;
            }

            if (form.Files.Count > 0)
            {
                var archivo = form.Files[0];
                var nombreArchivo = $"{Guid.NewGuid()}_{Path.GetFileName(archivo.FileName)}";
                var rutaCarpeta = Path.Combine(_env.WebRootPath, "documentos");

                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                documento.RutaArchivo = $"documentos/{nombreArchivo}";
                documento.FechaSubida = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST sin archivo (no se recomienda usar en producción)
        [HttpPost("subir")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubirDocumento([FromBody] DocumentoEmpleadoCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Usuario no autenticado.");

            var usuario = await _context.Usuarios
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userIdClaim));

            if (usuario == null || usuario.EmpleadoId == null)
                return BadRequest("No se pudo identificar al empleado asociado.");

            var documento = new DocumentoEmpleado
            {
                EmpleadoId = usuario.EmpleadoId.Value,
                TipoDocumentoId = dto.TipoDocumentoId,
                RutaArchivo = dto.RutaArchivo,
                FechaSubida = DateTime.Now
            };

            _context.DocumentosEmpleado.Add(documento);
            await _context.SaveChangesAsync();

            return Ok("Documento guardado.");
        }

        // DELETE: Eliminar documento por ID
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
