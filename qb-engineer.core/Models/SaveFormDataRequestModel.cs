namespace QBEngineer.Core.Models;

public record SaveFormDataRequestModel(string FormDataJson, int? FormDefinitionVersionId = null);
