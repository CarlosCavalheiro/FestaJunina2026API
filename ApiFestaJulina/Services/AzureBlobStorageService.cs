using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ApiFestaJulina.Services;

public class AzureBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        var containerName = configuration["AzureBlobStorage:ContainerName"] ?? "uploads";
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task UploadFileAsync(Stream content, string folder, string fileName, string? contentType = null)
    {
        var blobClient = GetBlobClient(folder, fileName);
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        await blobClient.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
                }
            }
        );
    }

    public void UploadBytes(byte[] content, string folder, string fileName, string? contentType = null)
    {
        var blobClient = GetBlobClient(folder, fileName);
        _containerClient.CreateIfNotExists(PublicAccessType.None);

        blobClient.Upload(
            BinaryData.FromBytes(content),
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
                }
            }
        );
    }

    public async Task DeleteIfExistsAsync(string folder, string fileName)
    {
        var blobClient = GetBlobClient(folder, fileName);
        await blobClient.DeleteIfExistsAsync();
    }

    public string GetBlobUrl(string folder, string fileName)
    {
        return GetBlobClient(folder, fileName).Uri.ToString();
    }

    private BlobClient GetBlobClient(string folder, string fileName)
    {
        var blobName = $"{folder.Trim('/')}/{fileName}";
        return _containerClient.GetBlobClient(blobName);
    }
}
