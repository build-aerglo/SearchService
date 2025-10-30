using Microsoft.AspNetCore.Mvc;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController: ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "Search Service API is running 🚀" });
    }
}