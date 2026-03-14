namespace QBEngineer.Core.Models;

public record UserComplianceDetailResponseModel(
    int UserId,
    string UserName,
    string UserEmail,
    List<ComplianceFormSubmissionResponseModel> Submissions,
    List<IdentityDocumentResponseModel> IdentityDocuments);
