using Microsoft.AspNetCore.Http;
using ProyectoNomina.Backend.Services;

namespace ProyectoNomina.Backend.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _uploadPath;

        public bool IsEnabled => true;

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _env = env;
            _uploadPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "Uploads");
            
            // Crear directorio si no existe
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadAsync(IFormFile file, string blobPath, CancellationToken ct = default)
        {
            try
            {
                var fullPath = Path.Combine(_uploadPath, blobPath);
                var directory = Path.GetDirectoryName(fullPath);
                
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream, ct);
                
                return blobPath; // Retorna la ruta relativa
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al subir archivo: {ex.Message}", ex);
            }
        }

        public Task<string> GetReadSasUrlAsync(string blobPath, TimeSpan? lifetime = null, CancellationToken ct = default)
        {
            // Para archivos locales, simplemente retorna la URL relativa
            var url = $"/Uploads/{blobPath.Replace('\\', '/')}";
            return Task.FromResult(url);
        }

        public Task<bool> ExistsAsync(string blobPath, CancellationToken ct = default)
        {
            var fullPath = Path.Combine(_uploadPath, blobPath);
            return Task.FromResult(File.Exists(fullPath));
        }

        public Task DeleteAsync(string blobPath, CancellationToken ct = default)
        {
            try
            {
                var fullPath = Path.Combine(_uploadPath, blobPath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al eliminar archivo: {ex.Message}", ex);
            }
        }
    }
}