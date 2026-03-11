namespace QBEngineer.Core.Models;

public class EasyPostOptions
{
    public const string SectionName = "EasyPost";

    public string ApiKey { get; set; } = string.Empty;
    public bool TestMode { get; set; } = true;
}
