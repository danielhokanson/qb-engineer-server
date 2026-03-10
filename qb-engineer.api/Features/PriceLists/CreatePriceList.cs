using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.PriceLists;

public record CreatePriceListCommand(
    string Name,
    string? Description,
    int? CustomerId,
    bool IsDefault,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo,
    List<CreatePriceListEntryModel> Entries) : IRequest<PriceListListItemModel>;

public class CreatePriceListValidator : AbstractValidator<CreatePriceListCommand>
{
    public CreatePriceListValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Entries).NotEmpty().WithMessage("At least one price entry is required");
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.PartId).GreaterThan(0);
            entry.RuleFor(e => e.UnitPrice).GreaterThanOrEqualTo(0);
            entry.RuleFor(e => e.MinQuantity).GreaterThan(0);
        });
    }
}

public class CreatePriceListHandler(IPriceListRepository repo)
    : IRequestHandler<CreatePriceListCommand, PriceListListItemModel>
{
    public async Task<PriceListListItemModel> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = new PriceList
        {
            Name = request.Name,
            Description = request.Description,
            CustomerId = request.CustomerId,
            IsDefault = request.IsDefault,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
        };

        foreach (var entry in request.Entries)
        {
            priceList.Entries.Add(new PriceListEntry
            {
                PartId = entry.PartId,
                UnitPrice = entry.UnitPrice,
                MinQuantity = entry.MinQuantity,
            });
        }

        await repo.AddAsync(priceList, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        return new PriceListListItemModel(
            priceList.Id, priceList.Name, priceList.Description,
            priceList.CustomerId, null, priceList.IsDefault, priceList.IsActive,
            priceList.Entries.Count, priceList.CreatedAt);
    }
}
