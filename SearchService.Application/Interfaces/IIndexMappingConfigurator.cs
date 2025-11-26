using Elastic.Clients.Elasticsearch.IndexManagement;

namespace SearchService.Application.Interfaces;

public interface IIndexMappingConfigurator<T> where T : class
{
    CreateIndexRequest ConfigureIndex(string indexName);
}