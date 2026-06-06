using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace ApiFestaJulina.Services;

public class AzureBlobStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string? _storagePublicBaseUrl;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var provider = configuration["Storage:Provider"];
        if (!string.Equals(provider, "S3", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Somente Storage:Provider=S3 (MinIO) e suportado nesta API.");
        }

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
    }

    public async Task UploadFileAsync(Stream content, string folder, string fileName, string? contentType = null)
    {
        await EnsureS3BucketExistsAsync();
        var objectKey = BuildObjectKey(folder, fileName);

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
        });
    }

    public void UploadBytes(byte[] content, string folder, string fileName, string? contentType = null)
    {
        using var stream = new MemoryStream(content);
        UploadFileAsync(stream, folder, fileName, contentType).GetAwaiter().GetResult();
    }

    public async Task DeleteIfExistsAsync(string folder, string fileName)
    {
        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = BuildObjectKey(folder, fileName)
        });
    }

    public string GetBlobUrl(string folder, string fileName)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_storagePublicBaseUrl)
            ? _s3Client.Config.ServiceURL
            : _storagePublicBaseUrl;

        return $"{baseUrl!.TrimEnd('/')}/{_bucketName}/{BuildObjectKey(folder, fileName)}";
    }

    private static string BuildObjectKey(string folder, string fileName)
    {
        return $"{folder.Trim('/')}/{fileName}";
    }

    private async Task EnsureS3BucketExistsAsync()
    {
        if (await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName))
        {
            return;
        }

        await _s3Client.PutBucketAsync(new PutBucketRequest
        {
            BucketName = _bucketName
        });
    }
}
