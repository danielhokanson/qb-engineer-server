namespace QBEngineer.Core.Interfaces;

public interface ICurrencyService
{
    Task<decimal> GetExchangeRateAsync(int fromCurrencyId, int toCurrencyId, DateOnly date, CancellationToken ct);
    Task<decimal> ConvertAsync(decimal amount, int fromCurrencyId, int toCurrencyId, DateOnly date, CancellationToken ct);
    Task<int> GetBaseCurrencyIdAsync(CancellationToken ct);
    Task<decimal> CalculateExchangeGainLossAsync(decimal invoiceAmount, decimal invoiceRate, decimal paymentRate, CancellationToken ct);
    Task FetchExchangeRatesAsync(DateOnly date, CancellationToken ct);
}
