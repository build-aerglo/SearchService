namespace SearchService.Domain.Models;

public class FaqDto
{
    public string Question { get; set; } = default!;
    public string Answer { get; set; } = default!;
}