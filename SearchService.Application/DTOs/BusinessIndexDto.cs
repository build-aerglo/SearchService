// using SearchService.Infrastructure.Constants;
//
// namespace SearchService.Application.DTOs;
//
// public class BusinessIndexDto : IndexDto
// {
//     public string IndexName => IndexNames.Businesses;
//     public string Id => BusinessId.ToString();
//     
//     public Guid BusinessId { get; set; }
//     public string Name { get; set; } = string.Empty;
//     public string? Website { get; set; }
//     public bool IsBranch { get; set; }
//     public decimal? AvgRating { get; set; }
//     public int ReviewCount { get; set; }
//     public Guid? ParentBusinessId { get; set; }
//     public List<CategoryIndexDto> Categories { get; set; } = new();
//     public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
// }
//
// public class CategoryIndexDto
// {
//     public Guid Id { get; set; }
//     public string Name { get; set; } = string.Empty;
//     public string? Description { get; set; }
//     public Guid? ParentCategoryId { get; set; }
// }

using SearchService.Domain.Entities;
using SearchService.Infrastructure.Constants;

namespace SearchService.Application.DTOs;

public class BusinessIndexDto : IndexDto
{
    public string IndexName => IndexNames.Businesses;
    public string Id => BusinessId.ToString();
    
    public Guid BusinessId { get; set; }
    // public string Name { get; set; } = string.Empty;
    
    public User UserDto { get; set; }
    public BusinessRep BusinessRepDto { get; set; }
    // Remove all other properties for now
}