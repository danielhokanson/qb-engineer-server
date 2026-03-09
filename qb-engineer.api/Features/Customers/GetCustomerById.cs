using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Customers;

public record GetCustomerByIdQuery(int Id) : IRequest<CustomerDetailResponseModel>;

public class GetCustomerByIdHandler(ICustomerRepository repo)
    : IRequestHandler<GetCustomerByIdQuery, CustomerDetailResponseModel>
{
    public async Task<CustomerDetailResponseModel> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await repo.FindWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {request.Id} not found");

        return new CustomerDetailResponseModel(
            customer.Id,
            customer.Name,
            customer.CompanyName,
            customer.Email,
            customer.Phone,
            customer.IsActive,
            customer.ExternalId,
            customer.ExternalRef,
            customer.Provider,
            customer.CreatedAt,
            customer.UpdatedAt,
            customer.Contacts
                .OrderByDescending(c => c.IsPrimary)
                .ThenBy(c => c.LastName)
                .Select(c => new ContactResponseModel(
                    c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Role, c.IsPrimary))
                .ToList(),
            customer.Jobs
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new CustomerJobSummaryModel(
                    j.Id, j.JobNumber, j.Title,
                    j.CurrentStage?.Name, j.CurrentStage?.Color,
                    j.DueDate))
                .ToList());
    }
}
