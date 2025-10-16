namespace ProyectoNomina.Backend.Models
{
    public class DocumentoEmpleado
    {
        public int Id { get; set; }

        // Relaciones
        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; }

        public int TipoDocumentoId { get; set; }
        public TipoDocumento TipoDocumento { get; set; }

        // Archivo (compatibilidad con tu lógica actual)
        public string RutaArchivo { get; set; }
        public DateTime FechaSubida { get; set; }

        // ====== Metadata adicional (Paso 18) ======
        public string? NombreOriginal { get; set; }   // ej. "DPI_JuanPerez.pdf"
        public long? Tamano { get; set; }             // bytes
        public string? ContentType { get; set; }      // ej. "application/pdf"
        public string? Hash { get; set; }             // SHA-256 (hex)
        public int? SubidoPorUsuarioId { get; set; }  // FK opcional a Usuario
        public string? Observaciones { get; set; }    // notas RRHH
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}
