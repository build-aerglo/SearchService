using Dapr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Interfaces;
using SearchService.Domain.Events;

namespace SearchService.Api.Controllers;

// Dapr pub/sub endpoints. These are internal-only — the Dapr sidecar handles
// service-to-service mTLS in production. Do NOT expose these routes externally
// (block /events/* at your API gateway / ingress controller).
[ApiController]
[Route("events")]
[AllowAnonymous]
public class BusinessEventsController(IBusinessSearchService search) : ControllerBase
{
    [Topic("kafka-pubsub-business-created", "business-created")]
    [HttpPost("business-created")]
    public async Task<IActionResult> BusinessCreated([FromBody] BusinessCreatedEvent evt)
    {
        if (evt is null || evt.Id == Guid.Empty)
            return BadRequest("Invalid business-created event payload.");

        await search.HandleBusinessCreatedAsync(evt);
        return Ok();
    }

    [Topic("kafka-pubsub-business-deleted", "business-deleted")]
    [HttpPost("business-deleted")]
    public async Task<IActionResult> BusinessDeleted([FromBody] Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid business ID.");

        await search.HandleBusinessDeletedAsync(id);
        return Ok();
    }

    [Topic("kafka-pubsub-business-updated", "business-updated")]
    [HttpPost("business-updated")]
    public async Task<IActionResult> BusinessUpdated([FromBody] BusinessUpdatedEvent evt)
    {
        if (evt is null || evt.Id == Guid.Empty)
            return BadRequest("Invalid business-updated event payload.");

        await search.HandleBusinessUpdatedAsync(evt);
        return Ok();
    }
}
