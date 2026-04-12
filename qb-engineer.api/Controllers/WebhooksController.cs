using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Webhooks;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/admin/webhooks")]
[Authorize(Roles = "Admin")]
public class WebhooksController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<WebhookSubscriptionResponseModel>>> GetSubscriptions()
    {
        var result = await mediator.Send(new GetWebhookSubscriptionsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WebhookSubscriptionResponseModel>> CreateSubscription([FromBody] CreateWebhookSubscriptionRequestModel request)
    {
        var result = await mediator.Send(new CreateWebhookSubscriptionCommand(
            request.Url,
            request.EventTypesJson,
            request.Secret,
            request.Description,
            request.HeadersJson,
            request.MaxRetries,
            request.AutoDisableOnFailure));

        return CreatedAtAction(nameof(GetSubscriptions), new { }, result);
    }

    [HttpGet("{subscriptionId:int}/deliveries")]
    public async Task<ActionResult<List<WebhookDeliveryResponseModel>>> GetDeliveries(int subscriptionId)
    {
        var result = await mediator.Send(new GetWebhookDeliveriesQuery(subscriptionId));
        return Ok(result);
    }
}
