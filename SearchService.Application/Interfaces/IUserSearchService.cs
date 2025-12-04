using SearchService.Domain.Events;

namespace SearchService.Application.Interfaces;

public interface IUserSearchService
{
    Task HandleUserCreatedAsync(UserCreatedEvent evt);
}