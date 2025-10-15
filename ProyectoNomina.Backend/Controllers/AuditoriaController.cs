using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    // Por [Authorize] a nivel de clase, documentamos 401 y 403:
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AuditoriaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditoriaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        // Códigos para este GET (no-CRUD): 200 / 400 / 500
        [ProducesResponseType(typeof(IEnumerable<AuditoriaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AuditoriaDto>>> GetAuditoria([FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var baseQuery = _context.Auditoria.AsNoTracking();

    var total = await baseQuery.CountAsync();

    var auditoria = await baseQuery
        .OrderByDescending(a => a.Fecha)
        .ThenBy(a => a.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(a => new AuditoriaDto
        {
            Id = a.Id,
            Usuario = a.Usuario,
            Accion = a.Accion,
            Fecha = a.Fecha,
            Detalles = a.Detalles,
            Endpoint = a.Endpoint,
            Metodo = a.Metodo
        })
        .ToListAsync();

    Response.Headers["X-Total-Count"] = total.ToString();
    return Ok(auditoria);
        }
    }
}
