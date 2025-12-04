using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using SearchService.Domain.Models;

namespace SearchService.Infrastructure.Elasticsearch;

public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticsearchClient _client;

    public ElasticsearchService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<bool> TestConnectionAsync()
    {
        var ping = await _client.PingAsync();
        return ping.IsValidResponse;
    }

    public async Task<bool> CreateIndexAsync<T>(string indexName) where T : class
    {
        var exists = await _client.Indices.ExistsAsync(indexName);
        if (exists.Exists)
            return true;

        var create = await _client.Indices.CreateAsync(indexName);

        return create.IsValidResponse;
    }



    public async Task IndexDocumentAsync<T>(string indexName, T document) where T : class
    {
        await _client.IndexAsync(document, i => i.Index(indexName));
    }

    public async Task UpdateDocumentAsync<T>(string indexName, string id, T document) where T : class
    {
        await _client.UpdateAsync<T, T>(id, indexName, u => u.Doc(document));
    }

    public async Task DeleteDocumentAsync(string indexName, string id)
    {
        await _client.DeleteAsync(indexName, id);
    }

    public async Task<SearchResponseDto<T>> SearchDocumentsAsync<T>(SearchRequestDto request, string indexName) where T : class
    {
        var response = await _client.SearchAsync<T>(s => s
            .Index(indexName)
            .From((request.Page - 1) * request.PageSize)
            .Size(request.PageSize)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(request.Query)
                    .Fields("*")
                )
            )
        );

        return new SearchResponseDto<T>
        {
            Items = response.Documents,
            Total = response.Total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
