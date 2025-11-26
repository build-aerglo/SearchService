using SearchService.Domain.Events;

namespace SearchService.Application.Interfaces;

public interface IBusinessSearchService
{
    Task HandleBusinessCreatedAsync(BusinessCreatedEvent businessEvent);
    Task HandleBusinessUpdatedAsync(BusinessCreatedEvent businessEvent);
    Task HandleBusinessDeletedAsync(Guid businessId);
}