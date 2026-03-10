using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Admin;

public record UpsertSystemSettingsCommand(List<SystemSettingRequestModel> Settings) : IRequest<List<SystemSettingResponseModel>>;

public record SystemSettingRequestModel(string Key, string Value, string? Description);

public class UpsertSystemSettingsHandler(ISystemSettingRepository repo) : IRequestHandler<UpsertSystemSettingsCommand, List<SystemSettingResponseModel>>
{
    public async Task<List<SystemSettingResponseModel>> Handle(UpsertSystemSettingsCommand request, CancellationToken ct)
    {
        foreach (var item in request.Settings)
        {
            var existing = await repo.FindByKeyAsync(item.Key, ct);
            if (existing is not null)
            {
                existing.Value = item.Value;
                if (item.Description is not null)
                    existing.Description = item.Description;
            }
            else
            {
                await repo.AddAsync(new SystemSetting
                {
                    Key = item.Key,
                    Value = item.Value,
                    Description = item.Description,
                }, ct);
            }
        }

        await repo.SaveChangesAsync(ct);

        var all = await repo.GetAllAsync(ct);
        return all.Select(s => new SystemSettingResponseModel(s.Id, s.Key, s.Value, s.Description)).ToList();
    }
}
