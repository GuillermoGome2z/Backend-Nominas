using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidacionExpedienteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ValidacionExpedienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ValidacionExpediente/5
        [HttpGet("{empleadoId}")]
        public async Task<ActionResult<object>> VerificarExpediente(int empleadoId)
        {
            // Verifica si el empleado existe
            var empleado = await _context.Empleados.FindAsync(empleadoId);
            if (empleado == null) return NotFound("Empleado no encontrado.");

            // Total de tipos de documento requeridos
            var tiposRequeridos = await _context.TiposDocumento
                .Where(t => t.EsRequerido)
                .Select(t => t.Id)
                .ToListAsync();

            // Documentos que el empleado ha entregado
            var documentosEntregados = await _context.DocumentosEmpleado
                .Where(d => d.EmpleadoId == empleadoId)
                .Select(d => d.TipoDocumentoId)
                .Distinct()
                .ToListAsync();

            // Verifica cuáles faltan
            var faltantes = tiposRequeridos.Except(documentosEntregados).ToList();

            string estado = faltantes.Count == 0 ? "Completo"
                            : documentosEntregados.Count == 0 ? "Incompleto"
                            : "En proceso";

            return Ok(new
            {
                Empleado = empleado.NombreCompleto,
                EstadoExpediente = estado,
                DocumentosRequeridos = tiposRequeridos.Count,
                DocumentosPresentados = documentosEntregados.Count,
                DocumentosFaltantes = faltantes.Count
            });
        }
    }
}
