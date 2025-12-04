namespace SearchService.Domain.Models;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
}