using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Options;
using ProyectoNomina.Backend.Services;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/storage")]
    [Authorize(Roles = "Admin,RRHH")]
    public class StorageController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IFileStorageService _storage;
        private readonly AzureBlobOptions _opt;

        public StorageController(AppDbContext db, IFileStorageService storage, IOptions<AzureBlobOptions> opt)
        {
            _db = db;
            _storage = storage;
            _opt = opt.Value;
        }

        // POST api/storage/documentos/{documentoId}/upload
        // Sube el archivo a Azure Blob y actualiza RutaArchivo con la ruta del blob
        [HttpPost("documentos/{documentoId:int}/upload")]
        [RequestSizeLimit(104_857_600)] // 100 MB
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> UploadDocumentoBlob(int documentoId, IFormFile file, CancellationToken ct)
        {
            if (!_storage.IsEnabled) return StatusCode(StatusCodes.Status503ServiceUnavailable, "Azure Blob no está habilitado.");

            var doc = await _db.DocumentosEmpleado
                .Include(d => d.Empleado)
                .FirstOrDefaultAsync(d => d.Id == documentoId, ct);

            if (doc == null) return NotFound("Documento no encontrado.");
            if (file == null || file.Length == 0) return BadRequest("Archivo inválido.");

            // path recomendado: documentos/{empleadoId}/{documentoId}/{filename}
            var safeName = Path.GetFileName(file.FileName);
            var blobPath = $"documentos/{doc.EmpleadoId}/{doc.Id}/{safeName}";

            var storedPath = await _storage.UploadAsync(file, blobPath, ct);

            // guardamos la ruta del blob en RutaArchivo (compatible con tu modelo actual)
            doc.RutaArchivo = storedPath;
            doc.FechaSubida = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return Ok(new { documentoId = doc.Id, path = storedPath });
        }

        // GET api/storage/documentos/{documentoId}/sas?minutes=15
        // Devuelve URL firmada de lectura limitada en el tiempo
        [HttpGet("documentos/{documentoId:int}/sas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<SignedUrlDto>> GetDocumentoSasUrl(int documentoId, [FromQuery] int? minutes, CancellationToken ct)
        {
            if (!_storage.IsEnabled) return StatusCode(StatusCodes.Status503ServiceUnavailable, "Azure Blob no está habilitado.");

            var doc = await _db.DocumentosEmpleado.FirstOrDefaultAsync(d => d.Id == documentoId, ct);
            if (doc == null) return NotFound("Documento no encontrado.");
            if (string.IsNullOrWhiteSpace(doc.RutaArchivo)) return NotFound("El documento no tiene ruta de blob.");

            var lifespan = TimeSpan.FromMinutes(minutes.HasValue && minutes > 0 ? minutes.Value : _opt.DefaultSasMinutes);
            var url = await _storage.GetReadSasUrlAsync(doc.RutaArchivo, lifespan, ct);

            return Ok(new SignedUrlDto
            {
                Url = url,
                Path = doc.RutaArchivo,
                ExpiresAt = DateTimeOffset.UtcNow.Add(lifespan)
            });
        }
    }
}
