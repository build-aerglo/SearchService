using Elastic.Clients.Elasticsearch;

namespace SearchService.Application.Interfaces;

public interface IElasticsearchClientFactory
{
    ElasticsearchClient CreateClient();
}