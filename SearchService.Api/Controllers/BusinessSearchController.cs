using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using SearchService.Infrastructure.Constants;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessSearchController : ControllerBase
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<BusinessSearchController> _logger;

    public BusinessSearchController(
        IElasticsearchService elasticsearchService,
        ILogger<BusinessSearchController> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    [HttpPost("search")]
    public async Task<ActionResult<SearchResponseDto<BusinessIndexDto>>> Search([FromBody] SearchRequestDto request)
    {
        try
        {
            var result = await _elasticsearchService.SearchDocumentsAsync<BusinessIndexDto>(request, IndexNames.Businesses);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching businesses");
            return StatusCode(500, "An error occurred while searching businesses");
        }
    }

    // NEW: Get all indexed businesses (for verification)
    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyCollection<BusinessIndexDto>>> GetAllBusinesses()
    {
        try
        {
            var businesses = await _elasticsearchService.GetAllDocumentsAsync<BusinessIndexDto>(IndexNames.Businesses);
            return Ok(businesses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all businesses");
            return StatusCode(500, "An error occurred while getting businesses");
        }
    }

    // NEW: Get business by ID
    [HttpGet("{businessId}")]
    public async Task<ActionResult<BusinessIndexDto>> GetBusinessById(Guid businessId)
    {
        try
        {
            var request = new SearchRequestDto
            {
                Query = businessId.ToString(),
                PageSize = 1
            };
            
            var result = await _elasticsearchService.SearchDocumentsAsync<BusinessIndexDto>(request, IndexNames.Businesses);
            
            var business = result.Items.FirstOrDefault();
            if (business == null)
            {
                return NotFound($"Business with ID {businessId} not found");
            }

            return Ok(business);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business by ID {BusinessId}", businessId);
            return StatusCode(500, "An error occurred while getting business");
        }
    }

    // NEW: Get businesses by name
    [HttpGet("name/{businessName}")]
    public async Task<ActionResult<SearchResponseDto<BusinessIndexDto>>> GetBusinessesByName(string businessName)
    {
        try
        {
            var request = new SearchRequestDto
            {
                Query = businessName,
                PageSize = 50
            };
            
            var result = await _elasticsearchService.SearchDocumentsAsync<BusinessIndexDto>(request, IndexNames.Businesses);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting businesses by name {BusinessName}", businessName);
            return StatusCode(500, "An error occurred while searching businesses by name");
        }
    }

    // NEW: Check if index exists and get stats
    [HttpGet("status")]
    public async Task<ActionResult<object>> GetIndexStatus()
    {
        try
        {
            var indexExists = await _elasticsearchService.IndexExistsAsync(IndexNames.Businesses);
            
            if (!indexExists)
            {
                return Ok(new { indexExists = false, message = "Index does not exist" });
            }

            // Get some sample data to show count
            var businesses = await _elasticsearchService.GetAllDocumentsAsync<BusinessIndexDto>(IndexNames.Businesses);
            
            return Ok(new 
            { 
                indexExists = true, 
                documentCount = businesses.Count,
                indexName = IndexNames.Businesses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index status");
            return StatusCode(500, "An error occurred while getting index status");
        }
    }
}