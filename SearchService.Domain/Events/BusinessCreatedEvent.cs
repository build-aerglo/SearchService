using SearchService.Domain.Entities;

namespace SearchService.Domain.Events;

public class BusinessCreatedEvent
{
    public Guid Id { get; set; }
    // public string Name { get; set; } = string.Empty;

    public User UserDto { get; set; }
    public Guid BusinessId { get; set; }
    public BusinessRep BusinessRepDto { get; set; }
}
