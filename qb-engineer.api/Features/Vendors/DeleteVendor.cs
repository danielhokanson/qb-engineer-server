using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Vendors;

public record DeleteVendorCommand(int Id) : IRequest;

public class DeleteVendorHandler(IVendorRepository repo)
    : IRequestHandler<DeleteVendorCommand>
{
    public async Task Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Vendor {request.Id} not found");

        vendor.DeletedAt = DateTime.UtcNow;
        await repo.SaveChangesAsync(cancellationToken);
    }
}
