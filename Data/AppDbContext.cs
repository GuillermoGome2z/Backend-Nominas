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
        public DbSet<DetalleNominaHistorial> DetalleNominaHistorial { get; set; }
        
        // Nuevas tablas para sistema de nóminas completo
        public DbSet<ReglasLaborales> ReglasLaborales { get; set; }
        public DbSet<NominaDetalleLinea> NominaDetalleLineas { get; set; }
        public DbSet<ConceptoNomina> ConceptosNomina { get; set; }
        public DbSet<EmpleadoParametros> EmpleadoParametros { get; set; }
        public DbSet<NominaAportesPatronales> NominaAportesPatronales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===== Usuario =====
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(u => u.NombreCompleto)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(u => u.Correo)
                      .HasMaxLength(256)
                      .IsRequired();

                entity.Property(u => u.ClaveHash)
                      .IsRequired(); //  no nula

                entity.Property(u => u.Rol)
                      .HasMaxLength(50)
                      .IsRequired();

                //  Correo único
                entity.HasIndex(u => u.Correo).IsUnique();
            });

            // ===== Configuración de Empleados =====
            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.Property(e => e.NombreCompleto)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.Correo)
                      .HasMaxLength(256);

                entity.Property(e => e.Telefono)
                      .HasMaxLength(20);

                entity.Property(e => e.Direccion)
                      .HasMaxLength(500);

                entity.Property(e => e.DPI)
                      .HasMaxLength(13);

                entity.Property(e => e.NIT)
                      .HasMaxLength(15);

                entity.Property(e => e.EstadoLaboral)
                      .HasMaxLength(20)
                      .HasDefaultValue("ACTIVO");

                entity.Property(e => e.SalarioMensual)
                      .HasPrecision(18, 2);

                // Índices según especificación
                entity.HasIndex(e => e.DPI);
                entity.HasIndex(e => e.Correo);
            });

            // ===== Configuración de Departamentos =====
            modelBuilder.Entity<Departamento>(entity =>
            {
                entity.Property(d => d.Nombre)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(d => d.Activo)
                      .HasDefaultValue(true);

                // Índice para búsquedas por estado activo
                entity.HasIndex(d => d.Activo);
            });

            // ===== Configuración de Puestos =====
            modelBuilder.Entity<Puesto>(entity =>
            {
                entity.Property(p => p.Nombre)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(p => p.SalarioBase)
                      .HasPrecision(18, 2);

                entity.Property(p => p.Activo)
                      .HasDefaultValue(true);

                // Índices según especificación: Puesto(DepartamentoId, Activo)
                entity.HasIndex(p => new { p.DepartamentoId, p.Activo });
            });

            // ===== Relaciones =====
            
            // Puesto -> Departamento (Restrict para evitar borrados en cascada)
            modelBuilder.Entity<Puesto>()
                .HasOne(p => p.Departamento)
                .WithMany(d => d.Puestos)
                .HasForeignKey(p => p.DepartamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Empleado -> Departamento (Restrict)
            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Departamento)
                .WithMany(d => d.Empleados)
                .HasForeignKey(e => e.DepartamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Empleado -> Puesto (Restrict)
            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Puesto)
                .WithMany(p => p.Empleados)
                .HasForeignKey(e => e.PuestoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Clave compuesta para UsuarioRol
            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.UsuarioId, ur.RolId });

            // Relación 1:1 entre Usuario y Empleado (opcional)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Empleado)
                .WithOne(e => e.Usuario)
                .HasForeignKey<Usuario>(u => u.EmpleadoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // ===== RefreshToken =====
            modelBuilder.Entity<RefreshToken>(et =>
            {
                et.Property(p => p.Token).IsRequired().HasMaxLength(512);
                et.Property(p => p.Expira).IsRequired();

                et.HasIndex(p => p.Token).IsUnique(); //  Token único

                // Relación 1:N correcta con Usuario
                et.HasOne(rt => rt.Usuario)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);

                // Índice útil para búsquedas por usuario/estado/expiración
                et.HasIndex(p => new { p.UsuarioId, p.Revocado, p.Expira });
            });

            // === ObservacionExpediente ===
            modelBuilder.Entity<ObservacionExpediente>(et =>
            {
                et.ToTable("ObservacionesExpediente");
                et.HasKey(o => o.Id);

                et.Property(o => o.Texto)
                  .HasMaxLength(2000)
                  .IsRequired();

                et.Property(o => o.FechaCreacion)
                  .HasDefaultValueSql("GETUTCDATE()");

                // FK obligatoria -> Empleado (elige una sola política para evitar conflictos)
                et.HasOne<Empleado>()
                  .WithMany()
                  .HasForeignKey(o => o.EmpleadoId)
                  .OnDelete(DeleteBehavior.Restrict);

                // FK opcional -> DocumentoEmpleado (evitar multiple cascade paths)
                et.HasOne<DocumentoEmpleado>()
                  .WithMany()
                  .HasForeignKey(o => o.DocumentoEmpleadoId)
                  .OnDelete(DeleteBehavior.SetNull);

                et.HasIndex(o => new { o.EmpleadoId, o.DocumentoEmpleadoId });
                et.HasIndex(o => o.FechaCreacion);
            });

            // ===== ReglasLaborales =====
            modelBuilder.Entity<ReglasLaborales>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Pais).HasMaxLength(2).IsRequired();
                entity.Property(r => r.IsrEscalaJson).IsRequired();
                entity.Property(r => r.PoliticaRedondeo).HasMaxLength(10).IsRequired();
                
                entity.HasIndex(r => new { r.Pais, r.VigenteDesde, r.Activo });
            });

            // ===== NominaDetalleLinea =====
            modelBuilder.Entity<NominaDetalleLinea>(entity =>
            {
                entity.HasKey(l => l.Id);
                
                entity.HasOne(l => l.NominaDetalle)
                      .WithMany(d => d.Lineas)
                      .HasForeignKey(l => l.NominaDetalleId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(l => new { l.NominaDetalleId, l.Tipo, l.Orden });
            });

            // ===== ConceptoNomina =====
            modelBuilder.Entity<ConceptoNomina>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Codigo).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Nombre).HasMaxLength(200).IsRequired();
                
                entity.HasIndex(c => c.Codigo).IsUnique();
                entity.HasIndex(c => new { c.Tipo, c.Activo });
            });

            // ===== EmpleadoParametros =====
            modelBuilder.Entity<EmpleadoParametros>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                entity.HasOne(p => p.Empleado)
                      .WithOne(e => e.Parametros)
                      .HasForeignKey<EmpleadoParametros>(p => p.EmpleadoId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(p => new { p.EmpleadoId, p.Activo });
                entity.HasIndex(p => p.VigenteDesde);
            });

            // ===== NominaAportesPatronales =====
            modelBuilder.Entity<NominaAportesPatronales>(entity =>
            {
                entity.HasKey(a => a.Id);
                
                entity.HasOne(a => a.Nomina)
                      .WithOne(n => n.AportesPatronales)
                      .HasForeignKey<NominaAportesPatronales>(a => a.NominaId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(a => a.NominaId).IsUnique();
            });
            
            // ===== Nomina =====
            modelBuilder.Entity<Nomina>(entity =>
            {
                entity.HasKey(n => n.Id);
                
                // Índice único compuesto para evitar duplicados
                entity.HasIndex(n => new { n.Periodo, n.TipoNomina })
                      .IsUnique()
                      .HasFilter("[Periodo] IS NOT NULL AND [Estado] <> 'ANULADA'");
                
                // Índices adicionales para búsquedas
                entity.HasIndex(n => n.Estado);
                entity.HasIndex(n => new { n.Anio, n.Mes });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
