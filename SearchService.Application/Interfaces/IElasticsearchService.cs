using SearchService.Application.DTOs;

namespace SearchService.Application.Interfaces;

public interface IElasticsearchService
{
    Task<bool> TestConnectionAsync();
    Task<bool> CreateIndexAsync<T>(string indexName) where T : class;
    Task IndexDocumentAsync<T>(T document) where T : class, IndexDto;
    Task UpdateDocumentAsync<T>(T document) where T : class, IndexDto;
    Task DeleteDocumentAsync(string indexName, string documentId);
    Task<SearchResponseDto<T>> SearchDocumentsAsync<T>(SearchRequestDto request, string indexName) where T : class;
    Task<bool> IndexExistsAsync(string indexName);
    Task<IReadOnlyCollection<T>> GetAllDocumentsAsync<T>(string indexName) where T : class; // NEW
}