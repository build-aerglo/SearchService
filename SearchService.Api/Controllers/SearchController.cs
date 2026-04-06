using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SearchService.Application.Interfaces;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/search")]
[Authorize]
public class SearchController(
    IBusinessSearchService searchService,
    IElasticsearchHealthService healthService) : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "Query parameter is required." });

        if (query.Length > 500)
            return BadRequest(new { error = "Query must not exceed 500 characters." });

        if (page < 1)
            return BadRequest(new { error = "Page must be at least 1." });

        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = await searchService.SearchAsync(query.Trim(), page, pageSize);
        return Ok(result);
    }

    // Health endpoint — intentionally open so load balancers and monitoring tools can probe it.
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> Health()
    {
        var healthy = await healthService.IsHealthyAsync();
        return healthy
            ? Ok(new { status = "healthy", elasticsearch = true })
            : StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "degraded", elasticsearch = false });
    }
}
