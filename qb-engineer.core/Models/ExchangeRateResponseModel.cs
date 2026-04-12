using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record ExchangeRateResponseModel(
    int Id,
    int FromCurrencyId,
    string FromCurrencyCode,
    int ToCurrencyId,
    string ToCurrencyCode,
    decimal Rate,
    DateOnly EffectiveDate,
    ExchangeRateSource Source,
    DateTimeOffset? FetchedAt);
