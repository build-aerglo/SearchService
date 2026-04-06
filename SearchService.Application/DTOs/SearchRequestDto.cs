using System.ComponentModel.DataAnnotations;

namespace SearchService.Application.DTOs;

public class SearchRequestDto
{
    [Required]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Query must be between 1 and 500 characters.")]
    public string Query { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 25;
}
