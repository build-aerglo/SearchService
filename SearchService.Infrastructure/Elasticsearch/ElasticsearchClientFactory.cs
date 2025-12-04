using System;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System.Text;

namespace SearchService.Infrastructure.Elasticsearch;

public class ElasticsearchClientFactory : IElasticsearchClientFactory
{
    private readonly string _cloudId;
    private readonly string _apiKey;

    public ElasticsearchClientFactory(string cloudId, string apiKey)
    {
        _cloudId = cloudId ?? throw new ArgumentNullException(nameof(cloudId));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
    }

    public ElasticsearchClient CreateClient()
    {
        var nodeUri = DecodeElasticCloudId(_cloudId);

        var settings = new ElasticsearchClientSettings(nodeUri)
            .Authentication(new ApiKey(_apiKey))
            .DisableDirectStreaming();

        return new ElasticsearchClient(settings);
    }

    /// <summary>
    /// Decodes Elastic Cloud ID (Elastic Cloud SaaS)
    /// Example Cloud ID:
    ///   cluster-name:dXMtZWFzdC0yLmF3...etc (base64)
    /// </summary>
    private static Uri DecodeElasticCloudId(string cloudId)
    {
        var parts = cloudId.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid Cloud ID format.", nameof(cloudId));

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
        // Format: {host}${es_uuid}${kibana_uuid}
        var decodedParts = decoded.Split('$');

        if (decodedParts.Length < 2)
            throw new Exception("Invalid decoded Cloud ID.");

        var host = decodedParts[0];
        var esUuid = decodedParts[1];

        // Elastic Cloud standard URL:
        var url = $"https://{esUuid}.{host}";

        return new Uri(url);
    }
}