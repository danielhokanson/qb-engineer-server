using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.SalesTax;

public record UpdateSalesTaxRateCommand(int Id, CreateSalesTaxRateRequestModel Data) : IRequest<SalesTaxRateResponseModel>;

public class UpdateSalesTaxRateValidator : AbstractValidator<UpdateSalesTaxRateCommand>
{
    public UpdateSalesTaxRateValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Data.Rate).InclusiveBetween(0, 1);
    }
}

public class UpdateSalesTaxRateHandler(AppDbContext db) : IRequestHandler<UpdateSalesTaxRateCommand, SalesTaxRateResponseModel>
{
    public async Task<SalesTaxRateResponseModel> Handle(UpdateSalesTaxRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await db.SalesTaxRates.FindAsync([request.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Sales tax rate {request.Id} not found.");

        // If this is being set as default, clear other defaults
        if (request.Data.IsDefault && !rate.IsDefault)
        {
            await db.SalesTaxRates
                .Where(r => r.IsDefault && r.Id != request.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsDefault, false), cancellationToken);
        }

        rate.Name = request.Data.Name.Trim();
        rate.Code = request.Data.Code.Trim();
        rate.Rate = request.Data.Rate;
        rate.IsDefault = request.Data.IsDefault;
        rate.Description = request.Data.Description?.Trim();

        await db.SaveChangesAsync(cancellationToken);

        return new SalesTaxRateResponseModel(
            rate.Id, rate.Name, rate.Code, rate.Rate, rate.IsDefault, rate.IsActive, rate.Description);
    }
}
