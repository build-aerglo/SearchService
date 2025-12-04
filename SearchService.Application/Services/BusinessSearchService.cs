using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;
using SearchService.Domain.Constants;
using SearchService.Domain.Events;
using SearchService.Domain.Models;

namespace SearchService.Application.Services;

public class BusinessSearchService(
    IElasticsearchService elasticsearchService,
    ILogger<BusinessSearchService> logger)
    : IBusinessSearchService
{
    // ------------------------------------------------------------
    // BUSINESS CREATED EVENT → INDEX DOCUMENT
    // ------------------------------------------------------------
    public async Task HandleBusinessCreatedAsync(BusinessCreatedEvent businessEvent)
    {
        try
        {
            var dto = MapToIndexDto(businessEvent);

            logger.LogInformation(
                "Indexing new business {BusinessId} into {Index}",
                businessEvent.Id,
                IndexNames.Businesses
            );

            await elasticsearchService.IndexDocumentAsync(
                IndexNames.Businesses,
                dto
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to index business {BusinessId}",
                businessEvent.Id
            );
            throw;
        }
    }

    // ------------------------------------------------------------
    // BUSINESS UPDATED EVENT → UPDATE DOCUMENT
    // ------------------------------------------------------------
    public async Task HandleBusinessUpdatedAsync(BusinessUpdatedEvent businessEvent)
    {
        try
        {
            var dto = MapToIndexDto(businessEvent);

            await elasticsearchService.UpdateDocumentAsync(
                IndexNames.Businesses,
                businessEvent.Id.ToString(),
                dto
            );

            logger.LogInformation(
                "Updated business {BusinessId} in index {Index}",
                businessEvent.Id,
                IndexNames.Businesses
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to update business {BusinessId} in {Index}",
                businessEvent.Id,
                IndexNames.Businesses
            );
            throw;
        }
    }

    // ------------------------------------------------------------
    // BUSINESS DELETED EVENT → REMOVE DOCUMENT
    // ------------------------------------------------------------
    public async Task HandleBusinessDeletedAsync(Guid businessId)
    {
        try
        {
            await elasticsearchService.DeleteDocumentAsync(
                IndexNames.Businesses,
                businessId.ToString()
            );

            logger.LogInformation(
                "Deleted business {BusinessId} from {Index}",
                businessId,
                IndexNames.Businesses
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to delete business {BusinessId} from {Index}",
                businessId,
                IndexNames.Businesses
            );
            throw;
        }
    }

    // ------------------------------------------------------------
    // MAPPING — EVENTS → INDEX DTO
    // ------------------------------------------------------------

    private static BusinessIndexDto MapToIndexDto(BusinessCreatedEvent evt)
    {
        return new BusinessIndexDto
        {
            BusinessId = evt.Id,
            Name = evt.Name,
            Website = evt.Website,
            AvgRating = evt.AvgRating,
            ReviewCount = evt.ReviewCount,
            IsBranch = evt.IsBranch,
            Categories = evt.Categories
        };
    }

    private static BusinessIndexDto MapToIndexDto(BusinessUpdatedEvent evt)
    {
        return new BusinessIndexDto
        {
            BusinessId = evt.Id,
            Name = evt.Name,
            Website = evt.Website,
            AvgRating = evt.AvgRating,
            ReviewCount = evt.ReviewCount,
            IsBranch = evt.IsBranch,
            Categories = evt.Categories,

            // Additional indexed fields
            BusinessAddress = evt.BusinessAddress,
            Logo = evt.Logo,
            OpeningHours = evt.OpeningHours,
            BusinessEmail = evt.BusinessEmail,
            BusinessPhoneNumber = evt.BusinessPhoneNumber,
            CacNumber = evt.CacNumber,
            AccessUsername = evt.AccessUsername,
            AccessNumber = evt.AccessNumber,
            SocialMediaLinks = evt.SocialMediaLinks,
            BusinessDescription = evt.BusinessDescription,
            Media = evt.Media,
            IsVerified = evt.IsVerified,
            ReviewLink = evt.ReviewLink,
            PreferredContactMethod = evt.PreferredContactMethod,
            Highlights = evt.Highlights,
            Tags = evt.Tags,
            AverageResponseTime = evt.AverageResponseTime,
            ProfileClicks = evt.ProfileClicks,
            Faqs = evt.Faqs,
            QrCodeBase64 = evt.QrCodeBase64
        };
    }
}
