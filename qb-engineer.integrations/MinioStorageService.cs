using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;

    public MinioStorageService(IOptions<MinioOptions> options)
    {
        var opts = options.Value;
        _client = new MinioClient()
            .WithEndpoint(opts.Endpoint)
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
        return await _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
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
}
