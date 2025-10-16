using ProyectoNomina.Backend.Models;

namespace ProyectoNomina.Backend.Services
{
    public interface IDetalleNominaAuditService
    {
        /// <summary>
        /// Registra difs por campo entre el estado original y el estado nuevo de un DetalleNomina.
        /// Debes llamar esto ANTES de SaveChanges().
        /// </summary>
        Task AuditarAsync(DetalleNomina original, DetalleNomina actualizado);
    }
}
