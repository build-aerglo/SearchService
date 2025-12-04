using SearchService.Application.DTOs;
using SearchService.Domain.Models;

namespace SearchService.Application.Interfaces;

public interface IElasticsearchService
{
    Task<bool> TestConnectionAsync();

    Task<bool> CreateIndexAsync<T>(string indexName) where T : class;

    Task IndexDocumentAsync<T>(string indexName, T document) where T : class;

    Task UpdateDocumentAsync<T>(string indexName, string id, T document) where T : class;

    Task DeleteDocumentAsync(string indexName, string id);

    Task<SearchResponseDto<T>> SearchDocumentsAsync<T>(SearchRequestDto request, string indexName) where T : class;
}