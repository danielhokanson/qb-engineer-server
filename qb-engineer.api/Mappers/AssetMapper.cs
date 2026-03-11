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
    [MapperIgnoreSource(nameof(Asset.SourceJob))]
    [MapperIgnoreSource(nameof(Asset.SourcePart))]
    [MapProperty(nameof(Asset.SourceJob) + "." + nameof(Job.JobNumber), nameof(AssetResponseModel.SourceJobNumber))]
    [MapProperty(nameof(Asset.SourcePart) + "." + nameof(Part.PartNumber), nameof(AssetResponseModel.SourcePartNumber))]
    public static partial AssetResponseModel ToResponseModel(this Asset asset);
}
