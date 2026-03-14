namespace QBEngineer.Core.Models;

public record DocumentSigningSubmission(int SubmissionId, string SubmitUrl);

public record DocumentSigningSubmissionStatus(string Status, DateTime? CompletedAt);
