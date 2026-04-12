using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record SetExchangeRateCommand(SetExchangeRateRequestModel Request) : IRequest<ExchangeRateResponseModel>;

public class SetExchangeRateHandler(AppDbContext db) : IRequestHandler<SetExchangeRateCommand, ExchangeRateResponseModel>
{
    public async Task<ExchangeRateResponseModel> Handle(SetExchangeRateCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var fromCurrency = await db.Currencies.FindAsync(new object[] { request.FromCurrencyId }, cancellationToken)
            ?? throw new KeyNotFoundException($"Currency {request.FromCurrencyId} not found");

        var toCurrency = await db.Currencies.FindAsync(new object[] { request.ToCurrencyId }, cancellationToken)
            ?? throw new KeyNotFoundException($"Currency {request.ToCurrencyId} not found");

        var existing = await db.ExchangeRates
            .FirstOrDefaultAsync(r =>
                r.FromCurrencyId == request.FromCurrencyId &&
                r.ToCurrencyId == request.ToCurrencyId &&
                r.EffectiveDate == request.EffectiveDate, cancellationToken);

        if (existing is not null)
        {
            existing.Rate = request.Rate;
            existing.Source = ExchangeRateSource.Manual;
            existing.FetchedAt = null;
        }
        else
        {
            existing = new ExchangeRate
            {
                FromCurrencyId = request.FromCurrencyId,
                ToCurrencyId = request.ToCurrencyId,
                Rate = request.Rate,
                EffectiveDate = request.EffectiveDate,
                Source = ExchangeRateSource.Manual,
            };
            db.ExchangeRates.Add(existing);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new ExchangeRateResponseModel(
            existing.Id, existing.FromCurrencyId, fromCurrency.Code,
            existing.ToCurrencyId, toCurrency.Code,
            existing.Rate, existing.EffectiveDate, existing.Source, existing.FetchedAt);
    }
}
