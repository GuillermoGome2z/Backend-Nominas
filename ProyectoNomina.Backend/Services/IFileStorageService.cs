using Microsoft.AspNetCore.Http;

namespace ProyectoNomina.Backend.Services
{
    public interface IFileStorageService
    {
        bool IsEnabled { get; }
        Task<string> UploadAsync(IFormFile file, string blobPath, CancellationToken ct = default);
        Task<string> GetReadSasUrlAsync(string blobPath, TimeSpan? lifetime = null, CancellationToken ct = default);
        Task<bool> ExistsAsync(string blobPath, CancellationToken ct = default);
        Task DeleteAsync(string blobPath, CancellationToken ct = default);
    }
}
