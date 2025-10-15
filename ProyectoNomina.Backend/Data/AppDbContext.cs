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
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ObservacionExpediente> ObservacionesExpediente { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Índice único para correo de Usuario
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            // Clave compuesta para UsuarioRol
            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.UsuarioId, ur.RolId });

            // Relación 1:N entre Departamento y Empleado
            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Departamento)
                .WithMany(d => d.Empleados)
                .HasForeignKey(e => e.DepartamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación 1:1 entre Usuario y Empleado
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Empleado)
                .WithOne(e => e.Usuario)
                .HasForeignKey<Usuario>(u => u.EmpleadoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuración mínima para RefreshToken
            modelBuilder.Entity<RefreshToken>(et =>
            {
                et.Property(p => p.Token)
                  .IsRequired()
                  .HasMaxLength(512);

                et.Property(p => p.Expira)
                  .IsRequired();

                et.HasIndex(p => p.Token)
                  .IsUnique();

                et.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(p => p.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            // === Configuración para ObservacionExpediente ===
            modelBuilder.Entity<ObservacionExpediente>(et =>
            {
                et.ToTable("ObservacionesExpediente");
                et.HasKey(o => o.Id);

                et.Property(o => o.Texto)
                  .HasMaxLength(2000)
                  .IsRequired();

                et.Property(o => o.FechaCreacion)
                  .HasDefaultValueSql("GETUTCDATE()");

                // FK obligatoria -> Empleado
                et.HasOne<Empleado>()
                  .WithMany()
                  .HasForeignKey(o => o.EmpleadoId)
                  .OnDelete(DeleteBehavior.Cascade);

                // FK obligatoria -> Usuario (creador)
               et.HasOne<Empleado>()
  .WithMany()
  .HasForeignKey(o => o.EmpleadoId)
  .OnDelete(DeleteBehavior.Restrict);

                // FK opcional -> DocumentoEmpleado (evitar multiple cascade paths)
                et.HasOne<DocumentoEmpleado>()
                  .WithMany()
                  .HasForeignKey(o => o.DocumentoEmpleadoId)
                  .OnDelete(DeleteBehavior.SetNull); // << cambio clave

                // Índices útiles
                et.HasIndex(o => new { o.EmpleadoId, o.DocumentoEmpleadoId });
                et.HasIndex(o => o.FechaCreacion);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
