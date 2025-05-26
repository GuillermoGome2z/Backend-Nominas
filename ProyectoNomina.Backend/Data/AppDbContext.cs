using Microsoft.EntityFrameworkCore;
using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UsuarioRol> UsuarioRoles { get; set; }

        // Agrega aquí el resto de tus entidades si ya las tienes
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Puesto> Puestos { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Nomina> Nominas { get; set; }
        public DbSet<DetalleNomina> DetalleNominas { get; set; }
        public DbSet<DocumentoEmpleado> DocumentosEmpleado { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Bonificacion> Bonificaciones { get; set; }
        public DbSet<Deduccion> Deducciones { get; set; }
        public DbSet<TipoDocumento> TiposDocumento { get; set; }
    }
}
