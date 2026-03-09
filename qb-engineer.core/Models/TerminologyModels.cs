namespace QBEngineer.Core.Models;

public record TerminologyEntryResponseModel(string Key, string Label);

public record UpdateTerminologyRequestModel(List<TerminologyEntryRequestModel> Entries);

public record TerminologyEntryRequestModel(string Key, string Label);
