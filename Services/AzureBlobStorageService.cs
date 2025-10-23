using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ProyectoNomina.Backend.Options;

namespace ProyectoNomina.Backend.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly AzureBlobOptions _opt;
        private readonly BlobServiceClient? _svc;
        private readonly BlobContainerClient? _container;

        public AzureBlobStorageService(IOptions<AzureBlobOptions> opt)
        {
            _opt = opt.Value;
            if (_opt.Enabled && !string.IsNullOrWhiteSpace(_opt.ConnectionString))
            {
                _svc = new BlobServiceClient(_opt.ConnectionString);
                _container = _svc.GetBlobContainerClient(_opt.ContainerName);
                _container.CreateIfNotExists(PublicAccessType.None);
            }
        }

        public bool IsEnabled => _opt.Enabled && _svc != null && _container != null;

        public async Task<string> UploadAsync(IFormFile file, string blobPath, CancellationToken ct = default)
        {
            if (!IsEnabled) throw new InvalidOperationException("Azure Blob no está habilitado.");
            var blob = _container!.GetBlobClient(NormalizePath(blobPath));
            await using var s = file.OpenReadStream();
            await blob.UploadAsync(s, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            }, ct);
            return blob.Name; // devolvemos la ruta interna del blob
        }

        public async Task<string> GetReadSasUrlAsync(string blobPath, TimeSpan? lifetime = null, CancellationToken ct = default)
        {
            if (!IsEnabled) throw new InvalidOperationException("Azure Blob no está habilitado.");
            var blob = _container!.GetBlobClient(NormalizePath(blobPath));
            if (!await blob.ExistsAsync(ct)) throw new FileNotFoundException("Blob no existe", blobPath);

            // SAS limitado a lectura por N minutos
            var expiresOn = DateTimeOffset.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(_opt.DefaultSasMinutes));
            var sasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, expiresOn)
            {
                BlobContainerName = _container.Name,
                BlobName = blob.Name
            };
            var uri = blob.GenerateSasUri(sasBuilder);
            return uri.ToString();
        }

        public async Task<bool> ExistsAsync(string blobPath, CancellationToken ct = default)
        {
            if (!IsEnabled) return false;
            var blob = _container!.GetBlobClient(NormalizePath(blobPath));
            var resp = await blob.ExistsAsync(ct);
            return resp.Value;
        }

        public async Task DeleteAsync(string blobPath, CancellationToken ct = default)
        {
            if (!IsEnabled) return;
            var blob = _container!.GetBlobClient(NormalizePath(blobPath));
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
        }

        private static string NormalizePath(string path)
        {
            path = path.Replace("\\", "/");
            while (path.StartsWith("/")) path = path[1..];
            return path;
        }
    }
}
