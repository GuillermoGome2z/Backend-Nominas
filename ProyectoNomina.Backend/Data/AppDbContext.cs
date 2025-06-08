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

        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Puesto> Puestos { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Nomina> Nominas { get; set; }
        public DbSet<DetalleNomina> DetalleNominas { get; set; }
        public DbSet<DocumentoEmpleado> DocumentosEmpleado { get; set; }
        public DbSet<Auditoria> Auditoria { get; set; }
        public DbSet<Bonificacion> Bonificaciones { get; set; }
        public DbSet<Deduccion> Deducciones { get; set; }
        public DbSet<TipoDocumento> TiposDocumento { get; set; }
        public DbSet<InformacionAcademica> InformacionAcademica { get; set; }
        public DbSet<AjusteManual> AjustesManuales { get; set; }
        public DbSet<Auditoria> AuditoriaLogs { get; set; }
        public DbSet<DetalleNomina> DetallesNomina { get; set; }
  



        // 🔑 CONFIGURACIÓN DE CLAVE COMPUESTA
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
        .HasIndex(u => u.Correo)
        .IsUnique();

            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.UsuarioId, ur.RolId });

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Empleado>()
       .HasOne(e => e.Departamento)
       .WithMany()
       .HasForeignKey(e => e.DepartamentoId)
       .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
