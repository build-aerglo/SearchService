using Elastic.Clients.Elasticsearch.IndexManagement;

namespace SearchService.Infrastructure.Elasticsearch.Mappings;

public interface IIndexMapping<T> where T : class
{
    CreateIndexRequest ConfigureIndex(string indexName);
}