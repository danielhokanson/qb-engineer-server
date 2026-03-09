using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Files;

public record DownloadFileQuery(int Id) : IRequest<FileDownloadResult>;

public record FileDownloadResult(Stream Stream, string ContentType, string FileName);

public class DownloadFileHandler(IFileRepository fileRepo, IStorageService storage) : IRequestHandler<DownloadFileQuery, FileDownloadResult>
{
    public async Task<FileDownloadResult> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        var file = await fileRepo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"File {request.Id} not found.");

        var stream = await storage.DownloadAsync(file.BucketName, file.ObjectKey, cancellationToken);

        return new FileDownloadResult(stream, file.ContentType, file.FileName);
    }
}
