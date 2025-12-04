using Elastic.Clients.Elasticsearch;

namespace SearchService.Infrastructure.Elasticsearch;

public interface IElasticsearchClientFactory
{
    ElasticsearchClient CreateClient();
}