namespace QBEngineer.Core.Models;

public class MinioOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public bool UseSsl { get; set; }
    public string JobFilesBucket { get; set; } = "qb-engineer-job-files";
    public string ReceiptsBucket { get; set; } = "qb-engineer-receipts";
    public string EmployeeDocsBucket { get; set; } = "qb-engineer-employee-docs";
    public string PiiDocsBucket { get; set; } = "qb-engineer-pii-docs";
}
