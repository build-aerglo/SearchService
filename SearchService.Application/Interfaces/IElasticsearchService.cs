using SearchService.Application.DTOs;

namespace SearchService.Application.Interfaces;

// Responsible for document CRUD and search operations against Elasticsearch.
// Health-check concerns are separated into IElasticsearchHealthService.
public interface IElasticsearchService
{
    Task<bool> CreateIndexAsync<T>(string indexName) where T : class;

    Task IndexDocumentAsync<T>(string indexName, T document) where T : class;

    Task UpdateDocumentAsync<T>(string indexName, string id, T document) where T : class;

    Task DeleteDocumentAsync(string indexName, string id);

    Task<SearchResponseDto<T>> SearchDocumentsAsync<T>(SearchRequestDto request, string indexName) where T : class;
}
