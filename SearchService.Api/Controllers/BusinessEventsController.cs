using Dapr;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Interfaces;
using SearchService.Domain.Events;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("events")]
public class BusinessEventsController(IBusinessSearchService search) : ControllerBase
{
    // Receive business-created JSON from Kafka via Dapr
    [Topic("kafka-pubsub-business-created", "business-created")]
    [HttpPost("business-created")]
    public async Task<IActionResult> BusinessCreated([FromBody] BusinessCreatedEvent evt)
    {
        await search.HandleBusinessCreatedAsync(evt);
        return Ok();
    }

    // Receive business-deleted JSON from Kafka via Dapr
    [Topic("kafka-pubsub-business-deleted", "business-deleted")]
    [HttpPost("business-deleted")]
    public async Task<IActionResult> BusinessDeleted([FromBody] Guid id)
    {
        await search.HandleBusinessDeletedAsync(id);
        return Ok();
    }
    
    // Receive business-updated JSON from Kafka via Dapr
    [Topic("kafka-pubsub-business-updated", "business-updated")]
    [HttpPost("business-updated")]
    public async Task<IActionResult> BusinessUpdated([FromBody] BusinessUpdatedEvent evt)
    {
        await search.HandleBusinessUpdatedAsync(evt);
        return Ok();
    }
}