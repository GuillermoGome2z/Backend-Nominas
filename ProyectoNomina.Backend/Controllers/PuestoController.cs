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
    public class PuestosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PuestosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Puestos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PuestoDto>>> GetPuestos()
        {
            var puestos = await _context.Puestos
                .Select(p => new PuestoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    SalarioBase = p.SalarioBase
                })
                .ToListAsync();

            return Ok(puestos);
        }

        // GET: api/Puestos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PuestoDto>> GetPuesto(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);

            if (puesto == null)
                return NotFound();

            var dto = new PuestoDto
            {
                Id = puesto.Id,
                Nombre = puesto.Nombre,
                SalarioBase = puesto.SalarioBase
            };

            return Ok(dto);
        }

        // POST: api/Puestos
        [HttpPost]
        public async Task<ActionResult<PuestoDto>> PostPuesto(PuestoDto dto)
        {
            var puesto = new Puesto
            {
                Nombre = dto.Nombre,
                SalarioBase = dto.SalarioBase
            };

            _context.Puestos.Add(puesto);
            await _context.SaveChangesAsync();

            dto.Id = puesto.Id;

            return CreatedAtAction(nameof(GetPuesto), new { id = dto.Id }, dto);
        }

        // PUT: api/Puestos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPuesto(int id, PuestoDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto == null)
                return NotFound();

            puesto.Nombre = dto.Nombre;
            puesto.SalarioBase = dto.SalarioBase;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Puestos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePuesto(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto == null)
                return NotFound();

            _context.Puestos.Remove(puesto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

