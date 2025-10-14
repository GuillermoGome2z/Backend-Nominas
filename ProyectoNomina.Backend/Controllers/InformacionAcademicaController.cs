using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,RRHH")]
    // Por [Authorize], documentamos 401 y 403 a nivel de clase
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class InformacionAcademicaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InformacionAcademicaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/InformacionAcademica
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<InformacionAcademicaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<InformacionAcademicaDto>>> GetInformacionAcademica()
        {
            var lista = await _context.InformacionAcademica
                .Include(i => i.Empleado)
                .Select(i => new InformacionAcademicaDto
                {
                    Id = i.Id,
                    EmpleadoId = i.EmpleadoId,
                    Titulo = i.Titulo,
                    Institucion = i.Institucion,
                    FechaGraduacion = i.FechaGraduacion
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET: api/InformacionAcademica/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InformacionAcademicaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<InformacionAcademicaDto>> GetInformacion(int id)
        {
            var info = await _context.InformacionAcademica.FindAsync(id);

            if (info == null)
                return NotFound();

            var dto = new InformacionAcademicaDto
            {
                Id = info.Id,
                EmpleadoId = info.EmpleadoId,
                Titulo = info.Titulo,
                Institucion = info.Institucion,
                FechaGraduacion = info.FechaGraduacion
            };

            return Ok(dto);
        }

        // POST: api/InformacionAcademica
        [HttpPost]
        [ProducesResponseType(typeof(InformacionAcademicaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<InformacionAcademicaDto>> PostInformacion(InformacionAcademicaDto dto)
        {
            var info = new InformacionAcademica
            {
                EmpleadoId = dto.EmpleadoId,
                Titulo = dto.Titulo,
                Institucion = dto.Institucion,
                FechaGraduacion = dto.FechaGraduacion,
                TipoCertificacion = "General" // puede venir luego como parte del DTO si se necesita
            };

            _context.InformacionAcademica.Add(info);
            await _context.SaveChangesAsync();

            dto.Id = info.Id;
            return CreatedAtAction(nameof(GetInformacion), new { id = info.Id }, dto);
        }

        // PUT: api/InformacionAcademica/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutInformacion(int id, InformacionAcademicaDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID de la URL no coincide con el del cuerpo.");

            var info = await _context.InformacionAcademica.FindAsync(id);
            if (info == null)
                return NotFound();

            info.Titulo = dto.Titulo;
            info.Institucion = dto.Institucion;
            info.FechaGraduacion = dto.FechaGraduacion;
            info.EmpleadoId = dto.EmpleadoId;

            _context.InformacionAcademica.Update(info);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/InformacionAcademica/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteInformacion(int id)
        {
            var info = await _context.InformacionAcademica.FindAsync(id);
            if (info == null)
                return NotFound();

            _context.InformacionAcademica.Remove(info);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
