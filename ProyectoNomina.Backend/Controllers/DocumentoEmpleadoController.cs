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
        public async Task<ActionResult<IEnumerable<DocumentoEmpleadoDto>>> GetDocumentos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var baseQuery = _context.DocumentosEmpleado
                .AsNoTracking()
                .Include(d => d.Empleado)
                .Include(d => d.TipoDocumento);

            var total = await baseQuery.CountAsync();

            var documentos = await baseQuery
                .OrderBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DocumentoEmpleadoDto
                {
                    Id = d.Id,
                    EmpleadoId = d.EmpleadoId,
                    TipoDocumentoId = d.TipoDocumentoId,
                    NombreTipo = d.TipoDocumento != null ? d.TipoDocumento.Nombre : null,
                    RutaArchivo = d.RutaArchivo,
                    FechaSubida = d.FechaSubida
                })
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(documentos);
        }

        // GET: /api/expedientes/{empleadoId}/documentos/{docId}
        [HttpGet("/api/expedientes/{empleadoId}/documentos/{docId}")]
        [Authorize(Roles = "Admin,RRHH,Empleado")]
        public async Task<IActionResult> DescargarDocumento(int empleadoId, int docId)
        {
            var doc = await _context.DocumentosEmpleado
                .FirstOrDefaultAsync(d => d.Id == docId && d.EmpleadoId == empleadoId);

            if (doc == null)
                return NotFound("Documento no encontrado.");

            if (!System.IO.File.Exists(doc.RutaArchivo))
                return NotFound("El archivo físico no existe en el servidor.");

            var ext = Path.GetExtension(doc.RutaArchivo);
            var contentType = GetContentType(ext);
            var downloadName = Path.GetFileName(doc.RutaArchivo);

            var bytes = await System.IO.File.ReadAllBytesAsync(doc.RutaArchivo);
            return File(bytes, contentType, downloadName);
        }

        // 🔹 Método auxiliar MIME
        private static string GetContentType(string ext)
        {
            return ext.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
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

        // POST /api/expedientes/{empleadoId}/documentos
        [HttpPost("/api/expedientes/{empleadoId}/documentos")]
        [Authorize(Roles = "Admin,RRHH")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> SubirDocumento(int empleadoId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se seleccionó ningún archivo.");

            var tiposPermitidos = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!tiposPermitidos.Contains(extension))
                return BadRequest($"Tipo de archivo no permitido ({extension}).");

            var rutaEmpleado = Path.Combine("Uploads", "Expedientes", empleadoId.ToString());
            Directory.CreateDirectory(rutaEmpleado);

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(rutaEmpleado, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var documento = new DocumentoEmpleado
            {
                EmpleadoId = empleadoId,
                RutaArchivo = rutaCompleta.Replace("\\", "/"),
                FechaSubida = DateTime.UtcNow,
                TipoDocumentoId = 0 // si no asignas tipo desde el front
            };

            _context.DocumentosEmpleado.Add(documento);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Archivo subido correctamente", documento.Id });
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
