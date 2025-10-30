namespace SearchService.Domain.Entities;

public sealed class Document
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = "";
    public string Content { get; private set; } = "";
    public DateTime CreatedAt { get; private set; }

    private Document() { }

    public Document(Guid id, string title, string content, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title required", nameof(title));
        Id = id;
        Title = title;
        Content = content ?? "";
        CreatedAt = createdAt;
    }

    public void UpdateContent(string newContent)
    {
        Content = newContent ?? "";
        // Optionally raise domain event
    }
}