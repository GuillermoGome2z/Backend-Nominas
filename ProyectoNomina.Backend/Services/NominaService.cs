using ProyectoNomina.Backend.Data;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Services
{
    public class NominaService
    {
        private readonly AppDbContext _context;

        public NominaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task Calcular(Nomina nomina)
        {
            var empleados = _context.Empleados.ToList();

            foreach (var empleado in empleados)
            {
                var salarioBruto = empleado.SalarioMensual;

                // Prestaciones legales
                var igss = salarioBruto * 0.0483m;
                var irtra = salarioBruto * 0.01m;
                var intecap = salarioBruto * 0.01m;

                var totalDeducciones = igss + irtra + intecap;

                // Bonificación incentivo fija (opcional personalizable)
                var bonificaciones = 250m;

                var salarioNeto = salarioBruto + bonificaciones - totalDeducciones;

                // Registrar detalle de nómina
                nomina.Detalles.Add(new DetalleNomina
                {
                    EmpleadoId = empleado.Id,
                    SalarioBruto = salarioBruto,
                    Deducciones = totalDeducciones,
                    Bonificaciones = bonificaciones,
                    SalarioNeto = salarioNeto,
                    DesgloseDeducciones = $"IGSS: {igss:C}, IRTRA: {irtra:C}, INTECAP: {intecap:C}"
                });
            }
        }
    }
}
