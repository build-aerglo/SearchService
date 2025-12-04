using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;

namespace SearchService.Infrastructure.Kafka;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration config, ILogger<KafkaProducer> logger)
    {
        _logger = logger;

        var bootstrapServers = config["Kafka:BootstrapServers"]
                               ?? throw new ArgumentNullException("Kafka:BootstrapServers is missing");

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishAsync(string topic, string message)
    {
        try
        {
            await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            });

            _logger.LogInformation("Kafka message published to {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing to Kafka topic {Topic}", topic);
            throw;
        }
    }
}
