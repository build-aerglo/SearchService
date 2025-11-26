using Microsoft.Extensions.Logging;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using SearchService.Domain.Events;
using SearchService.Infrastructure.Constants;

namespace SearchService.Application.Services;

public class BusinessSearchService : IBusinessSearchService
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<BusinessSearchService> _logger;

    public BusinessSearchService(
        IElasticsearchService elasticsearchService,
        ILogger<BusinessSearchService> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    public async Task HandleBusinessCreatedAsync(BusinessCreatedEvent businessEvent)
    {
        try
        {
            var businessIndex = MapToIndexDto(businessEvent);
            await _elasticsearchService.IndexDocumentAsync(businessIndex);
            
            _logger.LogInformation("Successfully indexed business {BusinessId}", businessEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index business {BusinessId}", businessEvent.Id);
            throw;
        }
    }

    public async Task HandleBusinessUpdatedAsync(BusinessCreatedEvent businessEvent)
    {
        try
        {
            var businessIndex = MapToIndexDto(businessEvent);
            await _elasticsearchService.UpdateDocumentAsync(businessIndex);
            
            _logger.LogInformation("Successfully updated business {BusinessId} in search index", businessEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update business {BusinessId} in search index", businessEvent.Id);
            throw;
        }
    }

    public async Task HandleBusinessDeletedAsync(Guid businessId)
    {
        try
        {
            await _elasticsearchService.DeleteDocumentAsync(IndexNames.Businesses, businessId.ToString());
            _logger.LogInformation("Successfully deleted business {BusinessId} from search index", businessId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete business {BusinessId} from search index", businessId);
            throw;
        }
    }

    private static BusinessIndexDto MapToIndexDto(BusinessCreatedEvent businessEvent)
    {
        return new BusinessIndexDto
        {
            BusinessId = businessEvent.Id,
            UserDto = businessEvent.UserDto,
            BusinessRepDto = businessEvent.BusinessRepDto
        };
    }
}