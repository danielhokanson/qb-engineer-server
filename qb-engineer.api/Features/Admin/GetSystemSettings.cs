using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Admin;

public record GetSystemSettingsQuery : IRequest<List<SystemSettingResponseModel>>;

public class GetSystemSettingsHandler(ISystemSettingRepository repo) : IRequestHandler<GetSystemSettingsQuery, List<SystemSettingResponseModel>>
{
    public async Task<List<SystemSettingResponseModel>> Handle(GetSystemSettingsQuery request, CancellationToken ct)
    {
        var settings = await repo.GetAllAsync(ct);
        return settings.Select(s => new SystemSettingResponseModel(s.Id, s.Key, s.Value, s.Description)).ToList();
    }
}
