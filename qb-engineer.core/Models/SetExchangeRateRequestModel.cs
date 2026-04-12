namespace QBEngineer.Core.Models;

public record SetExchangeRateRequestModel(
    int FromCurrencyId,
    int ToCurrencyId,
    decimal Rate,
    DateOnly EffectiveDate);
