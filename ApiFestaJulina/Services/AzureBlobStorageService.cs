using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ApiFestaJulina.Services;

public class AzureBlobStorageService
{
    private readonly BlobContainerClient? _containerClient;
    private readonly IAmazonS3? _s3Client;
    private readonly bool _useS3;
    private readonly string _bucketName;
    private readonly string? _storagePublicBaseUrl;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var provider = configuration["Storage:Provider"];
        _useS3 = string.Equals(provider, "S3", StringComparison.OrdinalIgnoreCase);

        if (_useS3)
        {
            _bucketName = configuration["Storage:BucketName"]
                ?? throw new InvalidOperationException("Configure Storage:BucketName para o provider S3.");

            var serviceUrl = configuration["Storage:ServiceUrl"]
                ?? throw new InvalidOperationException("Configure Storage:ServiceUrl para o provider S3.");

            var accessKey = configuration["Storage:AccessKey"]
                ?? throw new InvalidOperationException("Configure Storage:AccessKey para o provider S3.");

            var secretKey = configuration["Storage:SecretKey"]
                ?? throw new InvalidOperationException("Configure Storage:SecretKey para o provider S3.");

            _storagePublicBaseUrl = configuration["Storage:PublicBaseUrl"];

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var s3Config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(credentials, s3Config);
            return;
        }

        var containerName = configuration["AzureBlobStorage:ContainerName"] ?? "uploads";
        _bucketName = containerName;

        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        var serviceUri = configuration["AzureBlobStorage:ServiceUri"];

        BlobServiceClient blobServiceClient;

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            blobServiceClient = new BlobServiceClient(connectionString);
        }
        else if (!string.IsNullOrWhiteSpace(serviceUri))
        {
            blobServiceClient = new BlobServiceClient(new Uri(serviceUri), new DefaultAzureCredential());
        }
        else
        {
            throw new InvalidOperationException("Configure Storage:* para S3 ou AzureBlobStorage:* para Azure.");
        }

        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task UploadFileAsync(Stream content, string folder, string fileName, string? contentType = null)
    {
        if (_useS3)
        {
            await EnsureS3BucketExistsAsync();
            var objectKey = BuildObjectKey(folder, fileName);

            if (content.CanSeek)
            {
                content.Position = 0;
            }

            await _s3Client!.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = content,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
            });

            return;
        }

        var blobClient = GetBlobClient(folder, fileName);
        await _containerClient!.CreateIfNotExistsAsync(PublicAccessType.None);

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
        if (_useS3)
        {
            using var stream = new MemoryStream(content);
            UploadFileAsync(stream, folder, fileName, contentType).GetAwaiter().GetResult();
            return;
        }

        var blobClient = GetBlobClient(folder, fileName);
        _containerClient!.CreateIfNotExists(PublicAccessType.None);

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
        if (_useS3)
        {
            await _s3Client!.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = BuildObjectKey(folder, fileName)
            });

            return;
        }

        var blobClient = GetBlobClient(folder, fileName);
        await blobClient.DeleteIfExistsAsync();
    }

    public string GetBlobUrl(string folder, string fileName)
    {
        if (_useS3)
        {
            var baseUrl = string.IsNullOrWhiteSpace(_storagePublicBaseUrl)
                ? _s3Client!.Config.ServiceURL
                : _storagePublicBaseUrl;

            return $"{baseUrl!.TrimEnd('/')}/{_bucketName}/{BuildObjectKey(folder, fileName)}";
        }

        return GetBlobClient(folder, fileName).Uri.ToString();
    }

    private BlobClient GetBlobClient(string folder, string fileName)
    {
        var blobName = BuildObjectKey(folder, fileName);
        return _containerClient!.GetBlobClient(blobName);
    }

    private static string BuildObjectKey(string folder, string fileName)
    {
        return $"{folder.Trim('/')}/{fileName}";
    }

    private async Task EnsureS3BucketExistsAsync()
    {
        if (await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client!, _bucketName))
        {
            return;
        }

        await _s3Client!.PutBucketAsync(new PutBucketRequest
        {
            BucketName = _bucketName
        });
    }
}
