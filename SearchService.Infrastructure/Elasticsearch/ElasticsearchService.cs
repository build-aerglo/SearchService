using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using SearchService.Infrastructure.Constants;
using SearchService.Infrastructure.Elasticsearch.Mappings;

namespace SearchService.Infrastructure.Elasticsearch;

public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly Dictionary<Type, object> _indexMappings;

    public ElasticsearchService(IElasticsearchClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient();
        _indexMappings = RegisterIndexMappings();
    }
    
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await _client.PingAsync();
            if (response.IsValidResponse)
            {
                Console.WriteLine("✓ Successfully connected to Elasticsearch");
                return true;
            }
            else
            {
                Console.WriteLine($"✗ Elasticsearch connection failed: {response.DebugInformation}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Exception connecting to Elasticsearch: {ex.Message}");
            return false;
        }
    }

    // public async Task<bool> CreateIndexAsync<T>(string indexName) where T : class
    // {
    //     if (_indexMappings.TryGetValue(typeof(T), out var mapping))
    //     {
    //         var createIndexRequest = ((IIndexMapping<T>)mapping).ConfigureIndex(indexName);
    //         var response = await _client.Indices.CreateAsync(createIndexRequest);
    //         return response.IsValidResponse;
    //     }
    //
    //     // Fallback: create index with default mapping
    //     var fallbackResponse = await _client.Indices.CreateAsync(indexName);
    //     return fallbackResponse.IsValidResponse;
    // }
    
    public async Task<bool> CreateIndexAsync<T>(string indexName) where T : class
    {
        try
        {
            // First, check if index already exists
            var existsResponse = await _client.Indices.ExistsAsync(indexName);
            if (existsResponse.Exists)
            {
                Console.WriteLine($"Index '{indexName}' already exists");
                return true;
            }

            // SUPER SIMPLE: Create index with no mappings at all
            var response = await _client.Indices.CreateAsync(indexName);
        
            if (!response.IsValidResponse)
            {
                Console.WriteLine($"Failed to create index '{indexName}': {response.DebugInformation}");
                return false;
            }
        
            Console.WriteLine($"Index '{indexName}' created successfully (no custom mappings)");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception creating index '{indexName}': {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IndexExistsAsync(string indexName)
    {
        var response = await _client.Indices.ExistsAsync(indexName);
        return response.Exists;
    }

    public async Task IndexDocumentAsync<T>(T document) where T : class, IndexDto
    {
        // Ensure index exists
        if (!await IndexExistsAsync(document.IndexName))
        {
            await CreateIndexAsync<T>(document.IndexName);
        }

        var response = await _client.IndexAsync(document, i => i
            .Index(document.IndexName)
            .Id(document.Id));

        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to index document: {response.DebugInformation}");
        }
    }

    public async Task UpdateDocumentAsync<T>(T document) where T : class, IndexDto
    {
        var response = await _client.UpdateAsync<T, T>(document.IndexName, document.Id, u => u.Doc(document));
        
        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to update document: {response.DebugInformation}");
        }
    }

    public async Task DeleteDocumentAsync(string indexName, string documentId)
    {
        var response = await _client.DeleteAsync<object>(indexName, documentId);
        
        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to delete document: {response.DebugInformation}");
        }
    }

    public async Task<SearchResponseDto<T>> SearchDocumentsAsync<T>(SearchRequestDto request, string indexName) where T : class
    {
        // var from = (request.Page - 1) * request.PageSize;
        //
        // // SIMPLIFIED: Just basic text search on all fields
        // var searchResponse = await _client.SearchAsync<T>(s => s
        //     .Index(indexName)
        //     .From(from)
        //     .Size(request.PageSize)
        //     .Query(q => string.IsNullOrWhiteSpace(request.Query) 
        //         ? new MatchAllQuery() 
        //         : new MultiMatchQuery 
        //         { 
        //             Query = request.Query,
        //             Fields = new[] { "*" } // Search all fields
        //         })
        // );
        //
        // if (!searchResponse.IsValidResponse)
        // {
        //     throw new Exception($"Search failed: {searchResponse.DebugInformation}");
        // }
        //
        // return new SearchResponseDto<T>
        // {
        //     Items = searchResponse.Documents.ToList(),
        //     Total = searchResponse.Total,
        //     Page = request.Page,
        //     PageSize = request.PageSize
        // };
        return new SearchResponseDto<T>
        {
            Items = [],
            Total = 0,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    // NEW: Get all documents (for testing/verification)
    // public async Task<IReadOnlyCollection<T>> GetAllDocumentsAsync<T>(string indexName) where T : class
    // {
    //     var response = await _client.SearchAsync<T>(s => s
    //         .Index(indexName)
    //         .Query(q => new MatchAllQuery())
    //         .Size(1000) // Get up to 1000 documents
    //     );
    //
    //     if (!response.IsValidResponse)
    //     {
    //         throw new Exception($"Failed to get all documents: {response.DebugInformation}");
    //     }
    //
    //     return response.Documents;
    // }
    public async Task<IReadOnlyCollection<T>> GetAllDocumentsAsync<T>(string indexName) where T : class
    {
        try
        {
            var response = await _client.SearchAsync<T>(s => s
                .Index(indexName)
                .Query(q => q.MatchAll())  // FIXED: Use proper match_all query
                .Size(1000)
            );

            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to get all documents: {response.DebugInformation}");
            }

            return response.Documents;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get all documents: {ex.Message}", ex);
        }
    }

    private static Dictionary<Type, object> RegisterIndexMappings()
    {
        return new Dictionary<Type, object>
        {
            { typeof(BusinessIndexDto), new BusinessIndexMapping() }
        };
    }
}