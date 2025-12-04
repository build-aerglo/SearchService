using SearchService.Domain.Models;

namespace SearchService.Domain.Events;

public class BusinessCreatedEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Website { get; set; }
    public bool IsBranch { get; set; }
    public decimal AvgRating { get; set; }
    public long ReviewCount { get; set; }
    public Guid? ParentBusinessId { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
    public string? BusinessAddress { get; set; }
    public string? Logo { get; set; }
    public Dictionary<string, object>? OpeningHours { get; set; }
    public string? BusinessEmail { get; set; }
    public string? BusinessPhoneNumber { get; set; }
    public string? CacNumber { get; set; }
    public string? AccessUsername { get; set; }
    public string? AccessNumber { get; set; }
    public Dictionary<string, string>? SocialMediaLinks { get; set; }
    public string? BusinessDescription { get; set; }
    public List<string>? Media { get; set; }
    public bool IsVerified { get; set; }
    public string? ReviewLink { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string[]? Highlights { get; set; }
    public string[]? Tags { get; set; }
    public string? AverageResponseTime { get; set; }
    public long ProfileClicks { get; set; }
    public List<FaqDto>? Faqs { get; set; }
    public string? QrCodeBase64 { get; set; }
}