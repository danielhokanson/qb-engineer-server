using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IDocumentSigningService
{
    Task<bool> IsAvailableAsync(CancellationToken ct);
    Task<int> CreateTemplateFromPdfAsync(string name, byte[] pdfBytes, CancellationToken ct);
    Task<DocumentSigningSubmission> CreateSubmissionAsync(int templateId, string signerEmail, string signerName, CancellationToken ct);
    Task<byte[]> GetSignedPdfAsync(int submissionId, CancellationToken ct);
    Task<DocumentSigningSubmissionStatus> GetSubmissionStatusAsync(int submissionId, CancellationToken ct);
    Task DeleteTemplateAsync(int templateId, CancellationToken ct);
}
