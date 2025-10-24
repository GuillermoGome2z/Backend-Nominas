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
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class ExpedientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpedientesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/expedientes - Lista paginada con filtros
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetExpedientes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? departamentoId = null,
            [FromQuery] int? puestoId = null,
            [FromQuery] int? empleadoId = null,
            [FromQuery] string? estadoLaboral = null,
            [FromQuery] bool? activo = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = null)
        {
            try
            {
                var query = _context.Empleados
                    .Include(e => e.Departamento)
                    .Include(e => e.Puesto)
                    .Include(e => e.Documentos)
                    .AsQueryable();

                // Filtros
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(e => e.NombreCompleto.Contains(search) || 
                                           (e.Correo != null && e.Correo.Contains(search)));
                }

                if (departamentoId.HasValue)
                {
                    query = query.Where(e => e.DepartamentoId == departamentoId.Value);
                }

                if (puestoId.HasValue)
                {
                    query = query.Where(e => e.PuestoId == puestoId.Value);
                }

                if (empleadoId.HasValue)
                {
                    query = query.Where(e => e.Id == empleadoId.Value);
                }

                if (!string.IsNullOrWhiteSpace(estadoLaboral))
                {
                    query = query.Where(e => e.EstadoLaboral == estadoLaboral);
                }

                if (activo.HasValue)
                {
                    var estado = activo.Value ? "ACTIVO" : "INACTIVO";
                    query = query.Where(e => e.EstadoLaboral == estado);
                }

                // Paginación
                var total = await query.CountAsync();
                var empleados = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        Id = e.Id,
                        NombreCompleto = e.NombreCompleto,
                        Correo = e.Correo,
                        Telefono = e.Telefono,
                        EstadoLaboral = e.EstadoLaboral,
                        Departamento = e.Departamento != null ? e.Departamento.Nombre : "Sin Departamento",
                        Puesto = e.Puesto != null ? e.Puesto.Nombre : "Sin Puesto",
                        TotalDocumentos = e.Documentos != null ? e.Documentos.Count() : 0,
                        FechaContratacion = e.FechaContratacion,
                        Activo = e.EstadoLaboral == "ACTIVO"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    data = empleados,
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)total / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener expedientes", error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/expedientes - Crear nuevo expediente
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> CreateExpediente([FromBody] CreateExpedienteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar si ya existe un empleado con el mismo DPI o correo
                if (!string.IsNullOrWhiteSpace(dto.DPI))
                {
                    var existeDPI = await _context.Empleados.AnyAsync(e => e.DPI == dto.DPI);
                    if (existeDPI)
                    {
                        return BadRequest(new { message = "Ya existe un empleado con ese DPI" });
                    }
                }

                if (!string.IsNullOrWhiteSpace(dto.Correo))
                {
                    var existeCorreo = await _context.Empleados.AnyAsync(e => e.Correo == dto.Correo);
                    if (existeCorreo)
                    {
                        return BadRequest(new { message = "Ya existe un empleado con ese correo" });
                    }
                }

                var empleado = new Empleado
                {
                    NombreCompleto = dto.NombreCompleto,
                    Correo = dto.Correo,
                    Telefono = dto.Telefono,
                    Direccion = dto.Direccion,
                    SalarioMensual = dto.SalarioMensual,
                    FechaContratacion = dto.FechaContratacion,
                    DPI = dto.DPI,
                    NIT = dto.NIT,
                    FechaNacimiento = dto.FechaNacimiento,
                    EstadoLaboral = "ACTIVO",
                    DepartamentoId = dto.DepartamentoId,
                    PuestoId = dto.PuestoId
                };

                _context.Empleados.Add(empleado);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetExpediente), new { id = empleado.Id }, new
                {
                    Id = empleado.Id,
                    NombreCompleto = empleado.NombreCompleto,
                    Correo = empleado.Correo,
                    EstadoLaboral = empleado.EstadoLaboral,
                    FechaContratacion = empleado.FechaContratacion
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear expediente", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/expedientes/{id} - Obtener expediente específico
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetExpediente(int id)
        {
            try
            {
                var empleado = await _context.Empleados
                    .Include(e => e.Departamento)
                    .Include(e => e.Puesto)
                    .Include(e => e.Documentos)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (empleado == null)
                {
                    return NotFound(new { message = "Expediente no encontrado" });
                }

                return Ok(new
                {
                    Id = empleado.Id,
                    NombreCompleto = empleado.NombreCompleto,
                    Correo = empleado.Correo,
                    Telefono = empleado.Telefono,
                    Direccion = empleado.Direccion,
                    SalarioMensual = empleado.SalarioMensual,
                    FechaContratacion = empleado.FechaContratacion,
                    DPI = empleado.DPI,
                    NIT = empleado.NIT,
                    FechaNacimiento = empleado.FechaNacimiento,
                    EstadoLaboral = empleado.EstadoLaboral,
                    Departamento = empleado.Departamento?.Nombre,
                    DepartamentoId = empleado.DepartamentoId,
                    Puesto = empleado.Puesto?.Nombre,
                    PuestoId = empleado.PuestoId,
                    TotalDocumentos = empleado.Documentos?.Count() ?? 0,
                    Activo = empleado.EstadoLaboral == "ACTIVO"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener expediente", error = ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/expedientes/{id}/toggle - Cambiar estado activo/archivado
        /// </summary>
        [HttpPut("{id:int}/toggle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> ToggleEstado(int id)
        {
            try
            {
                var empleado = await _context.Empleados.FindAsync(id);
                if (empleado == null)
                {
                    return NotFound(new { message = "Expediente no encontrado" });
                }

                // Toggle entre ACTIVO y RETIRADO
                empleado.EstadoLaboral = empleado.EstadoLaboral == "ACTIVO" ? "RETIRADO" : "ACTIVO";
                
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Id = empleado.Id,
                    NombreCompleto = empleado.NombreCompleto,
                    EstadoLaboral = empleado.EstadoLaboral,
                    Activo = empleado.EstadoLaboral == "ACTIVO",
                    message = $"Estado cambiado a {empleado.EstadoLaboral}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cambiar estado", error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/expedientes/{id} - Eliminar expediente
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> DeleteExpediente(int id)
        {
            try
            {
                var empleado = await _context.Empleados
                    .Include(e => e.Documentos)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (empleado == null)
                {
                    return NotFound(new { message = "Expediente no encontrado" });
                }

                // Verificar si tiene documentos asociados
                if (empleado.Documentos?.Any() == true)
                {
                    return BadRequest(new { message = "No se puede eliminar un expediente con documentos asociados. Primero elimine los documentos." });
                }

                _context.Empleados.Remove(empleado);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Expediente eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar expediente", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/expedientes/stats - Estadísticas del dashboard
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var totalExpedientes = await _context.Empleados.CountAsync();
                var expedientesActivos = await _context.Empleados.CountAsync(e => e.EstadoLaboral == "ACTIVO");
                var expedientesInactivos = await _context.Empleados.CountAsync(e => e.EstadoLaboral != "ACTIVO");
                var totalDocumentos = await _context.DocumentosEmpleado.CountAsync();

                var expedientesPorDepartamento = await _context.Empleados
                    .Include(e => e.Departamento)
                    .GroupBy(e => e.Departamento!.Nombre)
                    .Select(g => new { Departamento = g.Key, Total = g.Count() })
                    .ToListAsync();

                var expedientesRecientes = await _context.Empleados
                    .OrderByDescending(e => e.FechaContratacion)
                    .Take(5)
                    .Select(e => new
                    {
                        Id = e.Id,
                        NombreCompleto = e.NombreCompleto,
                        FechaContratacion = e.FechaContratacion,
                        EstadoLaboral = e.EstadoLaboral
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalExpedientes = totalExpedientes,
                    ExpedientesActivos = expedientesActivos,
                    ExpedientesInactivos = expedientesInactivos,
                    TotalDocumentos = totalDocumentos,
                    ExpedientesPorDepartamento = expedientesPorDepartamento,
                    ExpedientesRecientes = expedientesRecientes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/expedientes/{id}/download - Descargar expediente completo o documentos específicos
        /// </summary>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DescargarExpediente(int id, [FromQuery] string? tipo = null, [FromQuery] int? documentoId = null)
        {
            try
            {
                var empleado = await _context.Empleados
                    .Include(e => e.Documentos)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (empleado == null)
                    return NotFound(new { message = "Expediente no encontrado" });

                // Si se especifica un documento específico
                if (documentoId.HasValue)
                {
                    var documento = empleado.Documentos?.FirstOrDefault(d => d.Id == documentoId.Value);
                    if (documento == null)
                        return NotFound(new { message = "Documento no encontrado" });

                    // Aquí deberías implementar la lógica real para obtener el archivo
                    // Por ahora, generar contenido de ejemplo
                    var contenidoDocumento = GenerarContenidoDocumento(documento);
                    var fileName = $"{documento.NombreOriginal ?? "Documento"}_{empleado.NombreCompleto.Replace(" ", "_")}.pdf";
                    return File(contenidoDocumento, "application/pdf", fileName);
                }

                // Si se especifica un tipo de documento
                if (!string.IsNullOrEmpty(tipo))
                {
                    var documentosTipo = empleado.Documentos?.Where(d => (d.NombreOriginal ?? "").ToLower().Contains(tipo.ToLower())).ToList();
                    if (documentosTipo?.Any() != true)
                        return NotFound(new { message = $"No se encontraron documentos de tipo '{tipo}'" });

                    // Generar un ZIP con todos los documentos del tipo especificado
                    var contenidoZip = GenerarZipDocumentos(documentosTipo, empleado.NombreCompleto);
                    var zipFileName = $"Expediente_{empleado.NombreCompleto.Replace(" ", "_")}_{tipo}.zip";
                    return File(contenidoZip, "application/zip", zipFileName);
                }

                // Descargar expediente completo
                var todosDocumentos = empleado.Documentos?.ToList() ?? new List<DocumentoEmpleado>();
                var expedienteCompleto = GenerarExpedienteCompleto(empleado, todosDocumentos);
                var expedienteFileName = $"Expediente_Completo_{empleado.NombreCompleto.Replace(" ", "_")}.pdf";
                return File(expedienteCompleto, "application/pdf", expedienteFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al descargar expediente", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/expedientes/estadisticas - Estadísticas de expedientes para dashboard
        /// </summary>
        [HttpGet("estadisticas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetEstadisticasExpedientes()
        {
            try
            {
                var totalExpedientes = await _context.Empleados.CountAsync();
                var expedientesActivos = await _context.Empleados.CountAsync(e => e.EstadoLaboral == "ACTIVO");
                var expedientesInactivos = await _context.Empleados.CountAsync(e => e.EstadoLaboral == "INACTIVO");
                
                var totalDocumentos = await _context.DocumentosEmpleado.CountAsync();
                var documentosPorEmpleado = await _context.Empleados
                    .Select(e => new {
                        Empleado = e.NombreCompleto,
                        TotalDocumentos = e.Documentos != null ? e.Documentos.Count() : 0
                    })
                    .Where(x => x.TotalDocumentos > 0)
                    .OrderByDescending(x => x.TotalDocumentos)
                    .Take(5)
                    .ToListAsync();

                var expedientesPorDepartamento = await _context.Empleados
                    .GroupBy(e => e.Departamento != null ? e.Departamento.Nombre : "Sin Departamento")
                    .Select(g => new {
                        Departamento = g.Key,
                        Total = g.Count(),
                        Activos = g.Count(e => e.EstadoLaboral == "ACTIVO"),
                        Inactivos = g.Count(e => e.EstadoLaboral == "INACTIVO")
                    })
                    .ToListAsync();

                return Ok(new
                {
                    resumen = new {
                        totalExpedientes = totalExpedientes,
                        expedientesActivos = expedientesActivos,
                        expedientesInactivos = expedientesInactivos,
                        totalDocumentos = totalDocumentos,
                        porcentajeActivos = totalExpedientes > 0 ? Math.Round((double)expedientesActivos / totalExpedientes * 100, 1) : 0
                    },
                    expedientesPorDepartamento = expedientesPorDepartamento,
                    documentosPorEmpleado = documentosPorEmpleado,
                    fechaConsulta = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas de expedientes", error = ex.Message });
            }
        }

        // Métodos auxiliares para generar contenido de descargas
        private byte[] GenerarContenidoDocumento(DocumentoEmpleado documento)
        {
            // Implementación básica - en producción deberías obtener el archivo real del storage
            var contenido = $@"DOCUMENTO: {documento.NombreOriginal ?? "Sin nombre"}
EMPLEADO: {documento.Empleado?.NombreCompleto ?? "N/A"}
TIPO: {documento.TipoDocumento?.Nombre ?? "N/A"}
FECHA SUBIDA: {documento.FechaSubida:dd/MM/yyyy}
RUTA: {documento.RutaArchivo ?? "N/A"}

Este es un documento de ejemplo generado por el sistema.
En una implementación real, aquí se retornaría el contenido del archivo almacenado.";

            return System.Text.Encoding.UTF8.GetBytes(contenido);
        }

        private byte[] GenerarZipDocumentos(List<DocumentoEmpleado> documentos, string nombreEmpleado)
        {
            // Implementación básica - en producción usarías System.IO.Compression
            var contenido = $@"EXPEDIENTE ZIP - {nombreEmpleado}
DOCUMENTOS INCLUIDOS:

{string.Join("\n", documentos.Select(d => $"- {d.NombreOriginal ?? "Sin nombre"} ({d.TipoDocumento?.Nombre ?? "N/A"})"))}

Total de documentos: {documentos.Count}
Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            return System.Text.Encoding.UTF8.GetBytes(contenido);
        }

        private byte[] GenerarExpedienteCompleto(Empleado empleado, List<DocumentoEmpleado> documentos)
        {
            var contenido = $@"EXPEDIENTE COMPLETO
===================

DATOS PERSONALES:
Nombre: {empleado.NombreCompleto}
DPI: {empleado.DPI ?? "N/A"}
NIT: {empleado.NIT ?? "N/A"}
Correo: {empleado.Correo ?? "N/A"}
Teléfono: {empleado.Telefono ?? "N/A"}
Dirección: {empleado.Direccion ?? "N/A"}
Fecha Nacimiento: {empleado.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "N/A"}

DATOS LABORALES:
Departamento: {empleado.Departamento?.Nombre ?? "N/A"}
Puesto: {empleado.Puesto?.Nombre ?? "N/A"}
Salario Mensual: Q{empleado.SalarioMensual:N2}
Fecha Contratación: {empleado.FechaContratacion:dd/MM/yyyy}
Estado Laboral: {empleado.EstadoLaboral}

DOCUMENTOS ({documentos.Count}):
{string.Join("\n", documentos.Select(d => $"- {d.NombreOriginal ?? "Sin nombre"} ({d.TipoDocumento?.Nombre ?? "N/A"}) - Subido: {d.FechaSubida:dd/MM/yyyy}"))}

Expediente generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            return System.Text.Encoding.UTF8.GetBytes(contenido);
        }
    }

    // DTOs
    public class CreateExpedienteDto
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public decimal SalarioMensual { get; set; }
        public DateTime FechaContratacion { get; set; } = DateTime.Now;
        public string? DPI { get; set; }
        public string? NIT { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public int? DepartamentoId { get; set; }
        public int? PuestoId { get; set; }
    }
}