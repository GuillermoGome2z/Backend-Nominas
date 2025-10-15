using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;
using System.Security.Claims;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,RRHH")]
    [Route("api/expedientes/{empleadoId:int}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class ObservacionesExpedienteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ObservacionesExpedienteController(AppDbContext context)
        {
            _context = context;
        }

        // Helper para obtener el nombre visible del usuario (usa NombreCompleto; si viene null/empty, usa Correo)
        private async Task<string> GetUsuarioNombreAsync(int usuarioId)
        {
            return await _context.Usuarios
                .Where(u => u.Id == usuarioId)
                .Select(u => string.IsNullOrWhiteSpace(u.NombreCompleto) ? u.Correo : u.NombreCompleto)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        // GET: api/expedientes/{empleadoId}/observaciones?texto=&desde=&hasta=&documentoId=&page=1&pageSize=10
        [HttpGet("observaciones")]
        [ProducesResponseType(typeof(IEnumerable<ObservacionExpedienteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll(
            int empleadoId,
            [FromQuery] string? texto,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery(Name = "documentoId")] int? documentoEmpleadoId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var exists = await _context.Empleados.AnyAsync(e => e.Id == empleadoId);
            if (!exists) return NotFound("Empleado no encontrado.");

            if (documentoEmpleadoId.HasValue)
            {
                var docOk = await _context.DocumentosEmpleado
                    .AnyAsync(d => d.Id == documentoEmpleadoId.Value && d.EmpleadoId == empleadoId);
                if (!docOk) return NotFound("Documento del empleado no encontrado.");
            }

            var query = _context.ObservacionesExpediente
                .Where(o => o.EmpleadoId == empleadoId)
                .AsQueryable();

            if (documentoEmpleadoId.HasValue)
                query = query.Where(o => o.DocumentoEmpleadoId == documentoEmpleadoId.Value);
            if (!string.IsNullOrWhiteSpace(texto))
                query = query.Where(o => o.Texto.Contains(texto));
            if (desde.HasValue)
                query = query.Where(o => o.FechaCreacion >= desde.Value.Date);
            if (hasta.HasValue)
                query = query.Where(o => o.FechaCreacion < hasta.Value.Date.AddDays(1));

            query = query.OrderByDescending(o => o.FechaCreacion).AsNoTracking();

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new ObservacionExpedienteDto
                {
                    Id = o.Id,
                    EmpleadoId = o.EmpleadoId,
                    DocumentoEmpleadoId = o.DocumentoEmpleadoId,
                    Texto = o.Texto,
                    UsuarioId = o.UsuarioId,
                    // AQUÍ EL CAMBIO: usamos NombreCompleto o Correo
                    UsuarioNombre = string.Empty, // lo llenamos abajo para no repetir subquery
                    FechaCreacion = o.FechaCreacion,
                    FechaActualizacion = o.FechaActualizacion
                })
                .ToListAsync();

            // Completar UsuarioNombre con una sola consulta por item
            // (si prefieres, puedes mantener la subquery en el Select, pero así evitamos el error y mantenemos claridad)
            for (int i = 0; i < items.Count; i++)
            {
                items[i].UsuarioNombre = await GetUsuarioNombreAsync(items[i].UsuarioId);
            }

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }

        // GET: api/expedientes/{empleadoId}/observaciones/{id}
        [HttpGet("observaciones/{id:int}")]
        [ProducesResponseType(typeof(ObservacionExpedienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int empleadoId, int id)
        {
            var obs = await _context.ObservacionesExpediente
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id && o.EmpleadoId == empleadoId);

            if (obs == null) return NotFound();

            var dto = new ObservacionExpedienteDto
            {
                Id = obs.Id,
                EmpleadoId = obs.EmpleadoId,
                DocumentoEmpleadoId = obs.DocumentoEmpleadoId,
                Texto = obs.Texto,
                UsuarioId = obs.UsuarioId,
                UsuarioNombre = await GetUsuarioNombreAsync(obs.UsuarioId),
                FechaCreacion = obs.FechaCreacion,
                FechaActualizacion = obs.FechaActualizacion
            };

            return Ok(dto);
        }

        // POST: api/expedientes/{empleadoId}/observaciones
        [HttpPost("observaciones")]
        [ProducesResponseType(typeof(ObservacionExpedienteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(int empleadoId, [FromBody] ObservacionExpedienteCreateDto dto)
        {
            var empleado = await _context.Empleados.FindAsync(empleadoId);
            if (empleado == null) return NotFound("Empleado no encontrado.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (dto.DocumentoEmpleadoId.HasValue)
            {
                var ok = await _context.DocumentosEmpleado
                    .AnyAsync(d => d.Id == dto.DocumentoEmpleadoId.Value && d.EmpleadoId == empleadoId);
                if (!ok) return NotFound("Documento del empleado no encontrado.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Usuario no autenticado.");
            var userId = int.Parse(userIdClaim);

            var entity = new ObservacionExpediente
            {
                EmpleadoId = empleadoId,
                DocumentoEmpleadoId = dto.DocumentoEmpleadoId,
                Texto = dto.Texto,
                UsuarioId = userId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Add(entity);

            _context.Auditoria.Add(new Auditoria
            {
                Accion = "CREATE_OBSERVACION",
                Usuario = User.Identity?.Name ?? $"UsuarioId:{userId}",
                Fecha = DateTime.UtcNow,
                Detalles = $"EmpleadoId={empleadoId}; DocumentoEmpleadoId={dto.DocumentoEmpleadoId}",
                Endpoint = HttpContext.Request.Path,
                Metodo = "POST"
            });

            await _context.SaveChangesAsync();

            var result = new ObservacionExpedienteDto
            {
                Id = entity.Id,
                EmpleadoId = entity.EmpleadoId,
                DocumentoEmpleadoId = entity.DocumentoEmpleadoId,
                Texto = entity.Texto,
                UsuarioId = entity.UsuarioId,
                UsuarioNombre = await GetUsuarioNombreAsync(entity.UsuarioId),
                FechaCreacion = entity.FechaCreacion
            };

            return CreatedAtAction(nameof(GetById), new { empleadoId, id = entity.Id }, result);
        }

        // PUT: api/expedientes/{empleadoId}/observaciones/{id}
        [HttpPut("observaciones/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int empleadoId, int id, [FromBody] ObservacionExpedienteUpdateDto dto)
        {
            var obs = await _context.ObservacionesExpediente
                .FirstOrDefaultAsync(o => o.Id == id && o.EmpleadoId == empleadoId);
            if (obs == null) return NotFound();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            obs.Texto = dto.Texto;
            obs.FechaActualizacion = DateTime.UtcNow;

            _context.Auditoria.Add(new Auditoria
            {
                Accion = "UPDATE_OBSERVACION",
                Usuario = User.Identity?.Name ?? $"UsuarioId:{User.FindFirst(ClaimTypes.NameIdentifier)?.Value}",
                Fecha = DateTime.UtcNow,
                Detalles = $"ObsId={id}; EmpleadoId={empleadoId}",
                Endpoint = HttpContext.Request.Path,
                Metodo = "PUT"
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/expedientes/{empleadoId}/observaciones/{id}
        [HttpDelete("observaciones/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int empleadoId, int id)
        {
            var obs = await _context.ObservacionesExpediente
                .FirstOrDefaultAsync(o => o.Id == id && o.EmpleadoId == empleadoId);
            if (obs == null) return NotFound();

            _context.ObservacionesExpediente.Remove(obs);

            _context.Auditoria.Add(new Auditoria
            {
                Accion = "DELETE_OBSERVACION",
                Usuario = User.Identity?.Name ?? $"UsuarioId:{User.FindFirst(ClaimTypes.NameIdentifier)?.Value}",
                Fecha = DateTime.UtcNow,
                Detalles = $"ObsId={id}; EmpleadoId={empleadoId}",
                Endpoint = HttpContext.Request.Path,
                Metodo = "DELETE"
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Atajos por documento
        [HttpGet("documentos/{documentoId:int}/observaciones")]
        public Task<IActionResult> GetByDocumento(
            int empleadoId, int documentoId,
            [FromQuery] string? texto, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            => GetAll(empleadoId, texto, desde, hasta, documentoId, page, pageSize);

        [HttpPost("documentos/{documentoId:int}/observaciones")]
        public async Task<IActionResult> CreateInDocumento(int empleadoId, int documentoId, [FromBody] ObservacionExpedienteCreateDto body)
        {
            body.DocumentoEmpleadoId = documentoId;
            return await Create(empleadoId, body);
        }
    }
}
