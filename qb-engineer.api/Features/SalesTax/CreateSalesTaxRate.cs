using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.SalesTax;

public record CreateSalesTaxRateCommand(CreateSalesTaxRateRequestModel Data) : IRequest<SalesTaxRateResponseModel>;

public class CreateSalesTaxRateValidator : AbstractValidator<CreateSalesTaxRateCommand>
{
    public CreateSalesTaxRateValidator()
    {
        RuleFor(x => x.Data.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Data.Rate).InclusiveBetween(0, 1).WithMessage("Rate must be between 0 and 1 (e.g. 0.07 for 7%).");
    }
}

public class CreateSalesTaxRateHandler(AppDbContext db) : IRequestHandler<CreateSalesTaxRateCommand, SalesTaxRateResponseModel>
{
    public async Task<SalesTaxRateResponseModel> Handle(CreateSalesTaxRateCommand request, CancellationToken cancellationToken)
    {
        var exists = await db.SalesTaxRates.AnyAsync(r => r.Code == request.Data.Code, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Tax rate with code '{request.Data.Code}' already exists.");

        var now = DateTimeOffset.UtcNow;
        var effectiveFrom = request.Data.EffectiveFrom ?? now;
        var stateCode = string.IsNullOrWhiteSpace(request.Data.StateCode) ? null : request.Data.StateCode.Trim().ToUpper();

        // End-date any currently-active rate for the same state (or default if no state)
        if (stateCode is not null)
        {
            await db.SalesTaxRates
                .Where(r => r.StateCode == stateCode && r.EffectiveTo == null && r.EffectiveFrom <= effectiveFrom)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.EffectiveTo, effectiveFrom), cancellationToken);
        }

        // If this is set as default, clear other defaults
        if (request.Data.IsDefault)
        {
            await db.SalesTaxRates
                .Where(r => r.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsDefault, false), cancellationToken);
        }

        var rate = new SalesTaxRate
        {
            Name = request.Data.Name.Trim(),
            Code = request.Data.Code.Trim(),
            StateCode = stateCode,
            Rate = request.Data.Rate,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            IsDefault = request.Data.IsDefault,
            Description = request.Data.Description?.Trim(),
        };

        db.SalesTaxRates.Add(rate);
        await db.SaveChangesAsync(cancellationToken);

        return new SalesTaxRateResponseModel(
            rate.Id, rate.Name, rate.Code, rate.StateCode, rate.Rate, rate.EffectiveFrom, rate.EffectiveTo, rate.IsDefault, rate.IsActive, rate.Description);
    }
}
