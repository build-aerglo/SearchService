using SearchService.Domain.Events;

namespace SearchService.Application.Interfaces;

public interface IBusinessSearchService
{
    Task HandleBusinessCreatedAsync(BusinessCreatedEvent evt);
    Task HandleBusinessDeletedAsync(Guid id);
    Task HandleBusinessUpdatedAsync(BusinessUpdatedEvent evt);
}