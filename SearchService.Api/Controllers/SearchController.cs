using Microsoft.AspNetCore.Mvc;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using SearchService.Domain.Constants;
using SearchService.Domain.Models;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController(IElasticsearchService es) : ControllerBase
{
    // -------------------------------------------------------
    // Basic Search: /api/search?query=text
    // -------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query is required");

        var request = new SearchRequestDto
        {
            Query = query,
            Page = 1,
            PageSize = 25
        };

        var result = await es.SearchDocumentsAsync<BusinessIndexDto>(
            request,
            IndexNames.Businesses
        );

        return Ok(result.Items);
    }


    // -------------------------------------------------------
    // Health Check — Elasticsearch Connectivity
    // -------------------------------------------------------
    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        var ok = await es.TestConnectionAsync();
        return Ok(new { elasticsearchConnected = ok });
    }
}