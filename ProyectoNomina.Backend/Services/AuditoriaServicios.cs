using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;

public class AuditoriaService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditoriaService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RegistrarAsync(string accion, string detalles)
    {
        var user = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistema";

        var auditoria = new Auditoria
        {
            Accion = accion,
            Usuario = user,
            Detalles = detalles,
            Fecha = DateTime.Now
        };

        _context.Auditorias.Add(auditoria);
        await _context.SaveChangesAsync();
    }
}
