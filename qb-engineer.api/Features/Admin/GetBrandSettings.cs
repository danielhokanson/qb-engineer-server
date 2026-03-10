using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Admin;

public record GetBrandSettingsQuery : IRequest<BrandSettingsResponseModel>;

public record BrandSettingsResponseModel(string? PrimaryColor, string? AccentColor, string? AppName);

public class GetBrandSettingsHandler(ISystemSettingRepository repo) : IRequestHandler<GetBrandSettingsQuery, BrandSettingsResponseModel>
{
    private static readonly string[] BrandKeys = ["theme.primary_color", "theme.accent_color", "app.name"];

    public async Task<BrandSettingsResponseModel> Handle(GetBrandSettingsQuery request, CancellationToken ct)
    {
        var all = await repo.GetAllAsync(ct);
        var lookup = all.Where(s => BrandKeys.Contains(s.Key)).ToDictionary(s => s.Key, s => s.Value);

        return new BrandSettingsResponseModel(
            PrimaryColor: lookup.GetValueOrDefault("theme.primary_color"),
            AccentColor: lookup.GetValueOrDefault("theme.accent_color"),
            AppName: lookup.GetValueOrDefault("app.name")
        );
    }
}
