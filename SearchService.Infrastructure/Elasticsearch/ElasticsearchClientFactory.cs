using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using SearchService.Application.Interfaces;

namespace SearchService.Infrastructure.Elasticsearch;

public class ElasticsearchClientFactory : IElasticsearchClientFactory
{
    private readonly ElasticsearchClient _client;

    public ElasticsearchClientFactory(string connectionString, string apiKey)
    {
        var settings = new ElasticsearchClientSettings(new Uri(connectionString))
            .DefaultIndex("businesses")
            .EnableDebugMode()
            .PrettyJson()
            .RequestTimeout(TimeSpan.FromMinutes(2));

        // Set the API key correctly using Authentication method
        settings.Authentication(new ApiKey(apiKey));

        _client = new ElasticsearchClient(settings);
    }

    public ElasticsearchClient CreateClient() => _client;
}