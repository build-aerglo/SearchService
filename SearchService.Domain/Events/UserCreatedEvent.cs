namespace SearchService.Domain.Events;

public class UserCreatedEvent
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string UserType { get; set; } = default!;
}