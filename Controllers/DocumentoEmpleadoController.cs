using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Models.DTOs;
using ProyectoNomina.Backend.Options;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    // Mantén la ruta plural para el FE estable:
    [Route("api/DocumentosEmpleado")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class DocumentoEmpleadoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IFileStorageService _storage;
        private readonly AzureBlobOptions _blobOpt;

        public DocumentoEmpleadoController(
            AppDbContext context,
            IWebHostEnvironment env,
            IFileStorageService storage,
            IOptions<AzureBlobOptions> blobOpt)
        {
            _context = context;
            _env = env;
            _storage = storage;
            _blobOpt = blobOpt.Value;
        }

        /// <summary>
        /// Subir archivo para expediente de empleado (multipart/form-data)
        /// </summary>
        /// <param name="empleadoId">ID del empleado</param>
        /// <param name="file">Archivo a subir (máximo 100 MB)</param>
        /// <param name="tipoDocumentoId">Tipo de documento</param>
        /// <returns>201 Created con metadatos, 413 si excede límite, 422 validaciones, 404 si empleado no existe</returns>
        /// <remarks>
        /// Límite de tamaño: 100 MB por archivo.
        /// Formatos soportados: PDF, DOC, DOCX, JPG, PNG, etc.
        /// 
        /// Ejemplo de form-data:
        /// - file: [archivo seleccionado]
        /// - tipoDocumentoId: 1
        /// 
        /// </remarks>
        [HttpPost("{empleadoId:int}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100_000_000)] // 100 MB → 413 si excede
        [ProducesResponseType(typeof(DocumentoEmpleadoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SubirDocumentoMultipart(
            int empleadoId,
            [FromForm] DocumentoUploadFormDto form,
            CancellationToken ct)
        {
            if (!_storage.IsEnabled)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Azure Blob no está habilitado.");

            var empleadoExists = await _context.Empleados.AnyAsync(e => e.Id == empleadoId, ct);
            if (!empleadoExists) return NotFound("Empleado no existe.");

            var tipoExists = await _context.TiposDocumento.AnyAsync(t => t.Id == form.TipoDocumentoId, ct);
            if (!tipoExists) return NotFound("Tipo de documento no existe.");

            var archivo = form.Archivo;
            if (archivo == null || archivo.Length == 0)
                return UnprocessableEntity(new { error = "Archivo vacío o ausente." });

            var allowedExt = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var allowedMime = new[] { "application/pdf", "image/jpeg", "image/png" };

            var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!allowedExt.Contains(ext))
                return UnprocessableEntity(new { error = $"Extensión no permitida: {ext}" });

            if (!allowedMime.Contains(archivo.ContentType))
                return UnprocessableEntity(new { error = $"MIME no permitido: {archivo.ContentType}" });

            static string Safe(string s) =>
                new string(s.Where(ch => char.IsLetterOrDigit(ch) || ch is '.' or '_' or '-' or ' ').ToArray()).Trim();

            var safeName = Safe(Path.GetFileName(archivo.FileName));
            var blobPath = $"documentos/{empleadoId}/{Guid.NewGuid():N}_{safeName}";

            // Hash SHA-256
            string sha256Hex;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var s = archivo.OpenReadStream())
            {
                var hash = await sha.ComputeHashAsync(s, ct);
                sha256Hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            var pathInBlob = await _storage.UploadAsync(archivo, blobPath, ct);

            var userIdStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            int? subidoPor = int.TryParse(userIdStr, out var uid) ? uid : (int?)null;

            var documento = new DocumentoEmpleado
            {
                EmpleadoId = empleadoId,
                TipoDocumentoId = form.TipoDocumentoId,
                RutaArchivo = pathInBlob,
                FechaSubida = DateTime.UtcNow,

                NombreOriginal = Path.GetFileName(archivo.FileName),
                Tamano = archivo.Length,
                ContentType = archivo.ContentType,
                Hash = sha256Hex,
                SubidoPorUsuarioId = subidoPor,
                CreadoEn = DateTime.UtcNow
            };

            _context.DocumentosEmpleado.Add(documento);
            _context.Auditoria.Add(new Auditoria
            {
                Usuario = User?.Identity?.Name ?? "sistema",
                Accion = "UploadDocumento",
                Detalles = $"EmpleadoId={empleadoId}, Tipo={form.TipoDocumentoId}, Ruta={pathInBlob}, Size={archivo.Length}, Ctype={archivo.ContentType}, Hash={sha256Hex}",
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);

            var dto = new DocumentoEmpleadoDto
            {
                Id = documento.Id,
                EmpleadoId = documento.EmpleadoId,
                TipoDocumentoId = documento.TipoDocumentoId,
                RutaArchivo = documento.RutaArchivo,
                FechaSubida = documento.FechaSubida
            };

            return CreatedAtAction(nameof(GetDocumento), new { id = dto.Id }, dto);
        }

        // GET /api/DocumentosEmpleado?empleadoId=&tipoDocumentoId=&from=&to=&page=&pageSize=
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DocumentoEmpleadoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DocumentoEmpleadoDto>>> GetDocumentos(
            [FromQuery] int? empleadoId,
            [FromQuery] int? tipoDocumentoId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

            var q = _context.DocumentosEmpleado
                .AsNoTracking()
                .Include(d => d.Empleado)
                .Include(d => d.TipoDocumento)
                .AsQueryable();

            if (empleadoId.HasValue) q = q.Where(d => d.EmpleadoId == empleadoId.Value);
            if (tipoDocumentoId.HasValue) q = q.Where(d => d.TipoDocumentoId == tipoDocumentoId.Value);
            if (from.HasValue) q = q.Where(d => d.FechaSubida >= from.Value);
            if (to.HasValue) q = q.Where(d => d.FechaSubida <= to.Value);

            var total = await q.CountAsync();

            var documentos = await q
                .OrderByDescending(d => d.FechaSubida)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DocumentoEmpleadoDto
                {
                    Id = d.Id,
                    EmpleadoId = d.EmpleadoId,
                    TipoDocumentoId = d.TipoDocumentoId,
                    NombreTipo = d.TipoDocumento != null ? d.TipoDocumento.Nombre : null,
                    RutaArchivo = d.RutaArchivo,
                    FechaSubida = d.FechaSubida,
                    NombreEmpleado = d.Empleado != null ? d.Empleado.NombreCompleto : null
                })
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(documentos);
        }

        /// <summary>
        /// Obtener lista de documentos de un empleado específico
        /// </summary>
        /// <param name="empleadoId">ID del empleado</param>
        /// <returns>200 con lista de documentos ordenados por fecha de subida (más recientes primero)</returns>
        [HttpGet("{empleadoId:int}")]
        [ProducesResponseType(typeof(IEnumerable<DocumentoEmpleadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DocumentoEmpleadoDto>>> GetDocumentosPorEmpleado(int empleadoId)
        {
            // Verificar que el empleado existe
            var empleadoExiste = await _context.Empleados.AnyAsync(e => e.Id == empleadoId);
            if (!empleadoExiste)
                return NotFound(new ProblemDetails { Title = "Empleado no encontrado", Detail = $"No existe un empleado con ID {empleadoId}." });

            var documentos = await _context.DocumentosEmpleado
                .AsNoTracking()
                .Include(d => d.TipoDocumento)
                .Where(d => d.EmpleadoId == empleadoId)
                .OrderByDescending(d => d.FechaSubida)
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

            return Ok(documentos);
        }

        // GET /api/DocumentosEmpleado/documento/{id} - Obtener documento por ID
        [HttpGet("documento/{id:int}")]
        [ProducesResponseType(typeof(DocumentoEmpleadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                NombreEmpleado = doc.Empleado?.NombreCompleto,
                NombreTipo = doc.TipoDocumento?.Nombre
            };
        }

        // GET /api/DocumentosEmpleado/{empleadoId}/{documentoId}/download?minutes=&download=
        [HttpGet("{empleadoId:int}/{documentoId:int}/download")]
        [ProducesResponseType(typeof(SignedUrlDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<SignedUrlDto>> GetSasDocumento(
            int empleadoId,
            int documentoId,
            [FromQuery] int? minutes,
            [FromQuery] bool download = false,
            CancellationToken ct = default)
        {
            if (!_storage.IsEnabled)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Azure Blob no está habilitado.");

            var doc = await _context.DocumentosEmpleado
                .FirstOrDefaultAsync(d => d.Id == documentoId && d.EmpleadoId == empleadoId, ct);

            if (doc == null) return NotFound("Documento no encontrado.");
            if (string.IsNullOrWhiteSpace(doc.RutaArchivo)) return NotFound("El documento no tiene ruta de blob.");

            var life = TimeSpan.FromMinutes(minutes.HasValue && minutes.Value > 0
                ? minutes.Value
                : _blobOpt.DefaultSasMinutes);

            var baseUrl = await _storage.GetReadSasUrlAsync(doc.RutaArchivo, life, ct);

            var finalUrl = baseUrl;
            if (download)
            {
                var filename = Path.GetFileName(doc.RutaArchivo);
                var cd = System.Net.WebUtility.UrlEncode($"attachment; filename=\"{filename}\"");
                finalUrl = baseUrl + (baseUrl.Contains('?') ? "&" : "?") + $"response-content-disposition={cd}";
            }

            _context.Auditoria.Add(new Auditoria
            {
                Usuario = User?.Identity?.Name ?? "sistema",
                Accion = "SasDocumento",
                Detalles = $"EmpleadoId={empleadoId}, DocumentoId={documentoId}, Min={life.TotalMinutes}, Download={download}",
                Fecha = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(ct);

            return Ok(new SignedUrlDto
            {
                Url = finalUrl,
                Path = doc.RutaArchivo,
                ExpiresAt = DateTimeOffset.UtcNow.Add(life)
            });
        }

        // PUT /api/DocumentosEmpleado/{id}  (actualiza solo TipoDocumentoId)
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ActualizarDocumento(int id, [FromBody] int tipoDocumentoId)
        {
            var doc = await _context.DocumentosEmpleado.FindAsync(id);
            if (doc == null) return NotFound();

            var tipoOk = await _context.TiposDocumento.AnyAsync(t => t.Id == tipoDocumentoId);
            if (!tipoOk) return UnprocessableEntity(new { error = "TipoDocumentoId inválido" });

            doc.TipoDocumentoId = tipoDocumentoId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/DocumentosEmpleado/{empleadoId}/{documentoId}
        [HttpDelete("{empleadoId:int}/{documentoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> DeleteDocumento(int empleadoId, int documentoId, CancellationToken ct)
        {
            if (!_storage.IsEnabled)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Azure Blob no está habilitado.");

            var doc = await _context.DocumentosEmpleado
                .FirstOrDefaultAsync(d => d.Id == documentoId && d.EmpleadoId == empleadoId, ct);

            if (doc == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(doc.RutaArchivo))
                await _storage.DeleteAsync(doc.RutaArchivo, ct);

            _context.DocumentosEmpleado.Remove(doc);
            _context.Auditoria.Add(new Auditoria
            {
                Usuario = User?.Identity?.Name ?? "sistema",
                Accion = "DeleteDocumento",
                Detalles = $"EmpleadoId={empleadoId}, DocumentoId={documentoId}, Ruta={(doc.RutaArchivo ?? "<sin-ruta>")}",
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        // ===== ENDPOINTS ANTIGUOS (local filesystem) =====

        [HttpPost("Upload")]
        [Obsolete("Usar POST /api/DocumentosEmpleado/{empleadoId} con Azure Blob")]
        public async Task<IActionResult> UploadDocumento([FromForm] DocumentoSubidaDto dto)
        {
            if (dto.Archivo == null || dto.Archivo.Length == 0)
                return BadRequest("Archivo no válido");

            var nombreArchivo = $"{Guid.NewGuid()}_{Path.GetFileName(dto.Archivo.FileName)}";
            var root = _env.WebRootPath ?? Directory.GetCurrentDirectory();
            var rutaCarpeta = Path.Combine(root, "documentos");
            Directory.CreateDirectory(rutaCarpeta);

            var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
            await using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await dto.Archivo.CopyToAsync(stream);
            }

            var documento = new DocumentoEmpleado
            {
                EmpleadoId = dto.EmpleadoId,
                TipoDocumentoId = dto.TipoDocumentoId,
                RutaArchivo = $"documentos/{nombreArchivo}",
                FechaSubida = DateTime.UtcNow
            };

            _context.DocumentosEmpleado.Add(documento);
            await _context.SaveChangesAsync();

            return Ok(new { documento.Id, documento.RutaArchivo });
        }

        [HttpGet("/api/expedientes/{empleadoId}/documentos/{docId}")]
        [Authorize(Roles = "Admin,RRHH,Empleado")]
        [Obsolete("Usar GET /api/DocumentosEmpleado/{empleadoId}/{documentoId}/download con SAS")]
        public async Task<IActionResult> DescargarDocumentoLocal(int empleadoId, int docId)
        {
            var doc = await _context.DocumentosEmpleado
                .FirstOrDefaultAsync(d => d.Id == docId && d.EmpleadoId == empleadoId);

            if (doc == null) return NotFound("Documento no encontrado.");
            if (string.IsNullOrWhiteSpace(doc.RutaArchivo))
                return NotFound("El documento no tiene ruta local válida.");

            if (Path.IsPathFullyQualified(doc.RutaArchivo))
                return BadRequest("Ruta de archivo inválida.");

            var fullPath = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), doc.RutaArchivo);
            if (!System.IO.File.Exists(fullPath))
                return NotFound("El archivo físico no existe en el servidor.");

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
            var downloadName = Path.GetFileName(fullPath);
            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, contentType, downloadName);
        }
    }
}
