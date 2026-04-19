using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record PurchaseOrderReceivedEvent(int PurchaseOrderId, int ReceivingRecordId, int UserId) : INotification;
