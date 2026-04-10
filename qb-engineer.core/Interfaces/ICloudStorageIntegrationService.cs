namespace QBEngineer.Core.Interfaces;

public record CloudStorageFile(
    string Path,
    string Name,
    long Size,
    string? ContentType,
    DateTimeOffset? ModifiedAt,
    bool IsFolder);

public interface ICloudStorageIntegrationService
{
    string ProviderId { get; }

    Task<string> UploadFileAsync(int userId, int integrationId, string remotePath, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadFileAsync(int userId, int integrationId, string remotePath, CancellationToken ct = default);
    Task<List<CloudStorageFile>> ListFilesAsync(int userId, int integrationId, string remotePath, CancellationToken ct = default);
    Task<string> GetShareLinkAsync(int userId, int integrationId, string remotePath, CancellationToken ct = default);
    Task DeleteFileAsync(int userId, int integrationId, string remotePath, CancellationToken ct = default);
    Task<bool> TestConnectionAsync(int userId, int integrationId, CancellationToken ct = default);
}
