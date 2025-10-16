using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;
using System.Globalization;

namespace ProyectoNomina.Backend.Services
{
    public class DetalleNominaAuditService : IDetalleNominaAuditService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _current;

        private static readonly HashSet<string> _camposAuditables =
            new(StringComparer.OrdinalIgnoreCase)
            {
                nameof(DetalleNomina.HorasRegulares),
                nameof(DetalleNomina.HorasExtra),
                nameof(DetalleNomina.TarifaHora),
                nameof(DetalleNomina.TarifaExtra),
                nameof(DetalleNomina.SalarioBruto),
                nameof(DetalleNomina.Deducciones),
                nameof(DetalleNomina.Bonificaciones),
                nameof(DetalleNomina.SalarioNeto),
                nameof(DetalleNomina.DesgloseDeducciones)
            };

        public DetalleNominaAuditService(AppDbContext context, ICurrentUserService current)
        {
            _context = context;
            _current = current;
        }

        public Task AuditarAsync(DetalleNomina original, DetalleNomina actualizado)
        {
            if (original == null || actualizado == null) return Task.CompletedTask;

            var usuarioId = _current.GetUserId();
            var ahora = DateTime.UtcNow;
            var lista = new List<DetalleNominaHistorial>();

            // Creamos un "snapshot" simple sin reflexión costosa
            // Compara sólo campos auditables (evita navegaciones)
            AddIfChanged(nameof(DetalleNomina.HorasRegulares), original.HorasRegulares, actualizado.HorasRegulares);
            AddIfChanged(nameof(DetalleNomina.HorasExtra),     original.HorasExtra,     actualizado.HorasExtra);
            AddIfChanged(nameof(DetalleNomina.TarifaHora),     original.TarifaHora,     actualizado.TarifaHora);
            AddIfChanged(nameof(DetalleNomina.TarifaExtra),    original.TarifaExtra,    actualizado.TarifaExtra);
            AddIfChanged(nameof(DetalleNomina.SalarioBruto),   original.SalarioBruto,   actualizado.SalarioBruto);
            AddIfChanged(nameof(DetalleNomina.Deducciones),    original.Deducciones,    actualizado.Deducciones);
            AddIfChanged(nameof(DetalleNomina.Bonificaciones), original.Bonificaciones, actualizado.Bonificaciones);
            AddIfChanged(nameof(DetalleNomina.SalarioNeto),    original.SalarioNeto,    actualizado.SalarioNeto);
            AddIfChanged(nameof(DetalleNomina.DesgloseDeducciones), original.DesgloseDeducciones, actualizado.DesgloseDeducciones);

            if (lista.Count > 0)
            {
                _context.AddRange(lista);
            }

            return Task.CompletedTask;

            void AddIfChanged<T>(string campo, T oldVal, T newVal)
            {
                if (!_camposAuditables.Contains(campo)) return;

                var sOld = ToStr(oldVal);
                var sNew = ToStr(newVal);
                if (sOld == sNew) return;

                lista.Add(new DetalleNominaHistorial
                {
                    DetalleNominaId = actualizado.Id, // si aún no tiene Id, debes asignarlo y llamar luego
                    Campo = campo,
                    ValorAnterior = sOld,
                    ValorNuevo = sNew,
                    UsuarioId = usuarioId,
                    Fecha = ahora
                });
            }

            static string? ToStr(object? v)
            {
                if (v == null) return null;
                return v switch
                {
                    decimal d => d.ToString("0.##", CultureInfo.InvariantCulture),
                    DateTime dt => dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    _ => v.ToString()
                };
            }
        }
    }
}
