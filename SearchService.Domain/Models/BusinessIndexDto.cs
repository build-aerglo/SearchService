namespace SearchService.Domain.Models;

public class BusinessIndexDto
{
    // Core identity
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = default!;
    public string? Website { get; set; }
    public bool IsBranch { get; set; }
    
    // Ratings
    public decimal AvgRating { get; set; }
    public long ReviewCount { get; set; }

    // Categories (names or IDs depending on your mapping)
    public List<CategoryDto> Categories { get; set; } = new();

    // Business details
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

    // Media & verification
    public List<string>? Media { get; set; }
    public bool IsVerified { get; set; }
    public string? ReviewLink { get; set; }
    public string? PreferredContactMethod { get; set; }

    // Arrays for search relevance
    public string[]? Highlights { get; set; }
    public string[]? Tags { get; set; }

    // Engagement
    public string? AverageResponseTime { get; set; }
    public long ProfileClicks { get; set; }

    // FAQs (optional but searchable)
    public List<FaqDto>? Faqs { get; set; }

    // QR Code
    public string? QrCodeBase64 { get; set; }
}