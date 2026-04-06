using SearchService.Application.DTOs;
using SearchService.Domain.Events;
using SearchService.Domain.Models;

namespace SearchService.Application.Interfaces;

public interface IBusinessSearchService
{
    Task<SearchResponseDto<BusinessSearchResultDto>> SearchAsync(string query, int page, int pageSize);
    Task HandleBusinessCreatedAsync(BusinessCreatedEvent evt);
    Task HandleBusinessDeletedAsync(Guid id);
    Task HandleBusinessUpdatedAsync(BusinessUpdatedEvent evt);
}
