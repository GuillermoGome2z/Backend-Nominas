using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class TipoDocumentoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TipoDocumentoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TipoDocumento
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TipoDocumentoDto>>> GetTipos()
        {
            var tipos = await _context.TiposDocumento
                .Select(t => new TipoDocumentoDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    EsRequerido = t.EsRequerido,
                    Orden = t.Orden
                })
                .ToListAsync();

            return Ok(tipos);
        }

        // GET: api/TipoDocumento/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TipoDocumentoDto>> GetTipoDocumento(int id)
        {
            var tipo = await _context.TiposDocumento.FindAsync(id);
            if (tipo == null)
                return NotFound();

            var dto = new TipoDocumentoDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre,
                Descripcion = tipo.Descripcion,
                EsRequerido = tipo.EsRequerido,
                Orden = tipo.Orden
            };

            return Ok(dto);
        }

        // ✅ POST usando DTO
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<TipoDocumentoDto>> PostTipoDocumento([FromBody] TipoDocumentoDto dto)
        {
            var tipo = new TipoDocumento
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                EsRequerido = dto.EsRequerido,
                Orden = dto.Orden
            };

            _context.TiposDocumento.Add(tipo);
            await _context.SaveChangesAsync();

            dto.Id = tipo.Id;
            return CreatedAtAction(nameof(GetTipoDocumento), new { id = tipo.Id }, dto);
        }

        // PUT: api/TipoDocumento/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PutTipoDocumento(int id, [FromBody] TipoDocumentoDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var tipo = await _context.TiposDocumento.FindAsync(id);
            if (tipo == null)
                return NotFound();

            tipo.Nombre = dto.Nombre;
            tipo.Descripcion = dto.Descripcion;
            tipo.EsRequerido = dto.EsRequerido;
            tipo.Orden = dto.Orden;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/TipoDocumento/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTipoDocumento(int id)
        {
            var tipo = await _context.TiposDocumento.FindAsync(id);
            if (tipo == null)
                return NotFound();

            _context.TiposDocumento.Remove(tipo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
