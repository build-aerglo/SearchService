using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;
using SearchService.Domain.Events;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessEventsController : ControllerBase
{
    private readonly IBusinessSearchService _businessSearchService;
    private readonly ILogger<BusinessEventsController> _logger;

    public BusinessEventsController(
        IBusinessSearchService businessSearchService,
        ILogger<BusinessEventsController> logger)
    {
        _businessSearchService = businessSearchService;
        _logger = logger;
    }

    [HttpPost("business-created")]
    [Topic("pubsub", "business-created")]
    public async Task<IActionResult> HandleBusinessCreated(BusinessCreatedEvent businessEvent)
    {
        try
        {
            await _businessSearchService.HandleBusinessCreatedAsync(businessEvent);
            _logger.LogInformation("Successfully processed business created event for {BusinessId}", businessEvent.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing business created event for {BusinessId}", businessEvent.Id);
            return StatusCode(500, "Error processing event");
        }
    }

    [HttpPost("business-updated")]
    [Topic("pubsub", "business-updated")]
    public async Task<IActionResult> HandleBusinessUpdated(BusinessCreatedEvent businessEvent)
    {
        try
        {
            await _businessSearchService.HandleBusinessUpdatedAsync(businessEvent);
            _logger.LogInformation("Successfully processed business updated event for {BusinessId}", businessEvent.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing business updated event for {BusinessId}", businessEvent.Id);
            return StatusCode(500, "Error processing event");
        }
    }

    [HttpPost("business-deleted")]
    [Topic("pubsub", "business-deleted")]
    public async Task<IActionResult> HandleBusinessDeleted(Guid businessId)
    {
        try
        {
            await _businessSearchService.HandleBusinessDeletedAsync(businessId);
            _logger.LogInformation("Successfully processed business deleted event for {BusinessId}", businessId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing business deleted event for {BusinessId}", businessId);
            return StatusCode(500, "Error processing event");
        }
    }
}