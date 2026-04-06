using Microsoft.Extensions.Logging;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using SearchService.Domain.Constants;
using SearchService.Domain.Events;
using SearchService.Domain.Models;

namespace SearchService.Application.Services;

public class BusinessSearchService(
    IElasticsearchService elasticsearchService,
    IBusinessIndexMapper mapper,
    ILogger<BusinessSearchService> logger)
    : IBusinessSearchService
{
    public async Task<SearchResponseDto<BusinessSearchResultDto>> SearchAsync(string query, int page, int pageSize)
    {
        var request = new SearchRequestDto
        {
            Query = query,
            Page = page,
            PageSize = pageSize
        };

        var esResult = await elasticsearchService.SearchDocumentsAsync<BusinessIndexDto>(request, IndexNames.Businesses);

        return new SearchResponseDto<BusinessSearchResultDto>
        {
            Items = esResult.Items.Select(ToSearchResult).ToList(),
            Total = esResult.Total,
            Page = esResult.Page,
            PageSize = esResult.PageSize
        };
    }

    public async Task HandleBusinessCreatedAsync(BusinessCreatedEvent businessEvent)
    {
        ArgumentNullException.ThrowIfNull(businessEvent);

        try
        {
            var dto = mapper.MapFromCreatedEvent(businessEvent);

            logger.LogInformation("Indexing new business {BusinessId} into {Index}",
                businessEvent.Id, IndexNames.Businesses);

            await elasticsearchService.IndexDocumentAsync(IndexNames.Businesses, dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to index business {BusinessId}", businessEvent.Id);
            throw;
        }
    }

    public async Task HandleBusinessUpdatedAsync(BusinessUpdatedEvent businessEvent)
    {
        ArgumentNullException.ThrowIfNull(businessEvent);

        try
        {
            var dto = mapper.MapFromUpdatedEvent(businessEvent);

            await elasticsearchService.UpdateDocumentAsync(
                IndexNames.Businesses,
                businessEvent.Id.ToString(),
                dto);

            logger.LogInformation("Updated business {BusinessId} in index {Index}",
                businessEvent.Id, IndexNames.Businesses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update business {BusinessId} in {Index}",
                businessEvent.Id, IndexNames.Businesses);
            throw;
        }
    }

    public async Task HandleBusinessDeletedAsync(Guid businessId)
    {
        try
        {
            await elasticsearchService.DeleteDocumentAsync(IndexNames.Businesses, businessId.ToString());

            logger.LogInformation("Deleted business {BusinessId} from {Index}",
                businessId, IndexNames.Businesses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete business {BusinessId} from {Index}",
                businessId, IndexNames.Businesses);
            throw;
        }
    }

    private static BusinessSearchResultDto ToSearchResult(BusinessIndexDto dto) => new()
    {
        BusinessId = dto.BusinessId,
        Name = dto.Name,
        Website = dto.Website,
        IsBranch = dto.IsBranch,
        AvgRating = dto.AvgRating,
        ReviewCount = dto.ReviewCount,
        Categories = dto.Categories,
        BusinessAddress = dto.BusinessAddress,
        Logo = dto.Logo,
        OpeningHours = dto.OpeningHours,
        BusinessEmail = dto.BusinessEmail,
        BusinessPhoneNumber = dto.BusinessPhoneNumber,
        SocialMediaLinks = dto.SocialMediaLinks,
        BusinessDescription = dto.BusinessDescription,
        Media = dto.Media,
        IsVerified = dto.IsVerified,
        ReviewLink = dto.ReviewLink,
        PreferredContactMethod = dto.PreferredContactMethod,
        Highlights = dto.Highlights,
        Tags = dto.Tags,
        AverageResponseTime = dto.AverageResponseTime,
        ProfileClicks = dto.ProfileClicks,
        Faqs = dto.Faqs
    };
}
