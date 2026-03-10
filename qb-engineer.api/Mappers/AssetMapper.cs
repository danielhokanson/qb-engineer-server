using Riok.Mapperly.Abstractions;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Mappers;

[Mapper]
public static partial class AssetMapper
{
    [MapperIgnoreSource(nameof(BaseAuditableEntity.DeletedAt))]
    [MapperIgnoreSource(nameof(BaseAuditableEntity.DeletedBy))]
    [MapperIgnoreSource(nameof(BaseAuditableEntity.IsDeleted))]
    public static partial AssetResponseModel ToResponseModel(this Asset asset);
}
