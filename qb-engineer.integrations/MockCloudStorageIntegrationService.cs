using Microsoft.Extensions.Logging;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Integrations;

public class MockCloudStorageIntegrationService(ILogger<MockCloudStorageIntegrationService> logger) : ICloudStorageIntegrationService
{
    public string ProviderId => "mock_storage";

    public Task<string> UploadFileAsync(int userId, int integrationId, string remotePath, Stream content, string contentType, CancellationToken ct = default)
    {
        logger.LogInformation("[MockCloudStorage] UploadFile '{Path}' ({ContentType}) for user {UserId}",
            remotePath, contentType, userId);
        return Task.FromResult($"https://mock-storage.local/{remotePath}");
    }

    public Task<Stream> DownloadFileAsync(int userId, int integrationId, string remotePath, CancellationToken ct = default)
    {
        logger.LogInformation("[MockCloudStorage] DownloadFile '{Path}' for user {UserId}", remotePath, userId);
        return Task.FromResult<Stream>(new MemoryStream());
    }

    public Task<List<CloudStorageFile>> ListFilesAsync(int userId, int integrationId, string remotePath, CancellationToken ct = default)
    {
        logger.LogInformation("[MockCloudStorage] ListFiles '{Path}' for user {UserId}", remotePath, userId);
        return Task.FromResult(new List<CloudStorageFile>());
    }

    public Task<string> GetShareLinkAsync(int userId, int integrationId, string remotePath, CancellationToken ct = default)
    {
        logger.LogInformation("[MockCloudStorage] GetShareLink '{Path}' for user {UserId}", remotePath, userId);
        return Task.FromResult($"https://mock-storage.local/share/{remotePath}");
    }

    public Task DeleteFileAsync(int userId, int integrationId, string remotePath, CancellationToken ct = default)
    {
        logger.LogInformation("[MockCloudStorage] DeleteFile '{Path}' for user {UserId}", remotePath, userId);
        return Task.CompletedTask;
    }

    public Task<bool> TestConnectionAsync(int userId, int integrationId, CancellationToken ct = default)
    {
        logger.LogInformation("[MockCloudStorage] TestConnection for user {UserId}", userId);
        return Task.FromResult(true);
    }
}
