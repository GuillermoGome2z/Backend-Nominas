using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [Authorize(Roles = "Admin,RRHH")]
    [Route("api/[controller]")]
    [ApiController]
    public class TipoDocumentoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TipoDocumentoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TipoDocumento
        [HttpGet]
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

            return tipos;
        }

        // GET: api/TipoDocumento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoDocumentoDto>> GetTipoDocumento(int id)
        {
            var tipo = await _context.TiposDocumento.FindAsync(id);
            if (tipo == null) return NotFound();

            var dto = new TipoDocumentoDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre,
                Descripcion = tipo.Descripcion,
                EsRequerido = tipo.EsRequerido,
                Orden = tipo.Orden
            };

            return dto;
        }

        // ✅ POST usando DTO
        [HttpPost]
        public async Task<ActionResult<TipoDocumentoDto>> PostTipoDocumento(TipoDocumentoDto dto)
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


        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoDocumento(int id, TipoDocumentoDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var tipo = await _context.TiposDocumento.FindAsync(id);
            if (tipo == null) return NotFound();

            tipo.Nombre = dto.Nombre;
            tipo.Descripcion = dto.Descripcion;
            tipo.EsRequerido = dto.EsRequerido;
            tipo.Orden = dto.Orden;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoDocumento(int id)
        {
            var tipo = await _context.TiposDocumento.FindAsync(id);
            if (tipo == null) return NotFound();

            _context.TiposDocumento.Remove(tipo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
