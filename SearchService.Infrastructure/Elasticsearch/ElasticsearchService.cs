using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;

namespace SearchService.Infrastructure.Elasticsearch;

public class ElasticsearchService : IElasticsearchService, IElasticsearchHealthService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchService> _logger;

    // Restrict multi-match to meaningful text fields only.
    // Searching Fields("*") would include internal/non-text fields and degrade relevance.
    private static readonly string[] SearchFields =
    [
        "name^3",               // highest boost — name match is most relevant
        "businessDescription",
        "businessAddress",
        "tags",
        "highlights",
        "categories.name",
        "faqs.question",
        "faqs.answer"
    ];

    public ElasticsearchService(ElasticsearchClient client, ILogger<ElasticsearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    // IElasticsearchHealthService
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var ping = await _client.PingAsync();
            return ping.IsValidResponse;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch health check failed");
            return false;
        }
    }

    // IElasticsearchService
    public async Task<bool> CreateIndexAsync<T>(string indexName) where T : class
    {
        var exists = await _client.Indices.ExistsAsync(indexName);
        if (exists.Exists)
            return true;

        var create = await _client.Indices.CreateAsync(indexName);
        if (!create.IsValidResponse)
            _logger.LogWarning("Failed to create Elasticsearch index {Index}", indexName);

        return create.IsValidResponse;
    }

    public async Task IndexDocumentAsync<T>(string indexName, T document) where T : class
    {
        var result = await _client.IndexAsync(document, i => i.Index(indexName));
        if (!result.IsValidResponse)
            _logger.LogWarning("Failed to index document in {Index}: {Error}", indexName, result.ElasticsearchServerError);
    }

    public async Task UpdateDocumentAsync<T>(string indexName, string id, T document) where T : class
    {
        var result = await _client.UpdateAsync<T, T>(id, indexName, u => u.Doc(document));
        if (!result.IsValidResponse)
            _logger.LogWarning("Failed to update document {Id} in {Index}: {Error}", id, indexName, result.ElasticsearchServerError);
    }

    public async Task DeleteDocumentAsync(string indexName, string id)
    {
        var result = await _client.DeleteAsync(indexName, id);
        if (!result.IsValidResponse)
            _logger.LogWarning("Failed to delete document {Id} from {Index}: {Error}", id, indexName, result.ElasticsearchServerError);
    }

    public async Task<SearchResponseDto<T>> SearchDocumentsAsync<T>(SearchRequestDto request, string indexName) where T : class
    {
        var response = await _client.SearchAsync<T>(s => s
            .Indices(indexName)
            .From((request.Page - 1) * request.PageSize)
            .Size(request.PageSize)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(request.Query)
                    .Fields(SearchFields)
                )
            )
        );

        if (!response.IsValidResponse)
        {
            _logger.LogWarning("Elasticsearch search returned invalid response for query '{Query}'", request.Query);
            return new SearchResponseDto<T> { Items = [], Total = 0, Page = request.Page, PageSize = request.PageSize };
        }

        return new SearchResponseDto<T>
        {
            Items = response.Documents,
            Total = response.Total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
