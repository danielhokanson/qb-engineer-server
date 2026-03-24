using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    // Separate client used only for presigned URL generation.
    // Presigned URLs embed the host in their HMAC signature, so the client used to
    // generate them must target the same host that the browser will request —
    // the public endpoint (e.g. localhost:9000) rather than the internal Docker hostname.
    private readonly IMinioClient _presignClient;

    public MinioStorageService(IOptions<MinioOptions> options)
    {
        var opts = options.Value;

        _client = new MinioClient()
            .WithEndpoint(opts.Endpoint)
            .WithCredentials(opts.AccessKey, opts.SecretKey)
            .WithSSL(opts.UseSsl)
            .Build();

        // When a public endpoint is configured, use it for presigned URLs so the
        // generated signature matches the host the browser actually sends.
        var presignEndpoint = !string.IsNullOrWhiteSpace(opts.PublicEndpoint)
            ? opts.PublicEndpoint
            : opts.Endpoint;

        _presignClient = new MinioClient()
            .WithEndpoint(presignEndpoint)
            .WithCredentials(opts.AccessKey, opts.SecretKey)
            .WithSSL(opts.UseSsl)
            .Build();
    }

    public async Task UploadAsync(string bucketName, string objectKey, Stream stream, string contentType, CancellationToken ct)
    {
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType), ct);
    }

    public async Task<Stream> DownloadAsync(string bucketName, string objectKey, CancellationToken ct)
    {
        var ms = new MemoryStream();
        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(ms)), ct);
        ms.Position = 0;
        return ms;
    }

    public async Task<string> GetPresignedUrlAsync(string bucketName, string objectKey, int expirySeconds, CancellationToken ct)
    {
        // Use _presignClient (pointed at public endpoint) so the HMAC signature is
        // computed against the same host the browser will request.
        return await _presignClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithExpiry(expirySeconds));
    }

    public async Task DeleteAsync(string bucketName, string objectKey, CancellationToken ct)
    {
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey), ct);
    }

    public async Task EnsureBucketExistsAsync(string bucketName, CancellationToken ct)
    {
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), ct);
        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), ct);
        }
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct)
    {
        try
        {
            await _client.ListBucketsAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
