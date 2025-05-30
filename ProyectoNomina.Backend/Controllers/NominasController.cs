using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.Services;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [ApiController]
    [Route("api/[controller]")]
    public class NominasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NominaService _nominaService;

        public NominasController(AppDbContext context, NominaService nominaService)
        {
            _context = context;
            _nominaService = nominaService;
        }

        // ✅ Obtener todas las nóminas con sus detalles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Nomina>>> GetNominas()
        {
            return await _context.Nominas
                .Include(n => n.Detalles)
                .ThenInclude(d => d.Empleado)
                .ToListAsync();
        }

        // ✅ Obtener una nómina específica por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Nomina>> GetNomina(int id)
        {
            var nomina = await _context.Nominas
                .Include(n => n.Detalles)
                .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(n => n.Id == id);

            return nomina == null ? NotFound() : nomina;
        }

        // ✅ Crear una nueva nómina (sin procesar)
        [HttpPost]
        public async Task<ActionResult<Nomina>> PostNomina([FromBody] Nomina nomina)
        {
            nomina.FechaGeneracion = DateTime.Now;
            _context.Nominas.Add(nomina);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNomina), new { id = nomina.Id }, nomina);
        }

        // ✅ Procesar y calcular automáticamente una nómina
        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarNomina([FromBody] string descripcion)
        {
            var empleados = await _context.Empleados.ToListAsync();
            if (!empleados.Any())
                return BadRequest("❌ No hay empleados disponibles para procesar la nómina.");

            var nomina = new Nomina
            {
                FechaGeneracion = DateTime.Now,
                Descripcion = descripcion
                // No inicializamos Detalles, lo hace NominaService
            };

            await _nominaService.Calcular(nomina); // Calcula los detalles y los llena

            _context.Nominas.Add(nomina);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "✅ Nómina procesada correctamente.",
                nominaId = nomina.Id
            });
        }

        // ✅ Editar una nómina existente
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNomina(int id, [FromBody] Nomina nomina)
        {
            if (id != nomina.Id) return BadRequest();

            _context.Entry(nomina).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Nominas.Any(n => n.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ✅ Eliminar una nómina por ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNomina(int id)
        {
            var nomina = await _context.Nominas.FindAsync(id);
            if (nomina == null) return NotFound();

            _context.Nominas.Remove(nomina);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
