namespace SearchService.Application.Interfaces;

public interface IKafkaProducer
{
    Task PublishAsync(string topic, string message);
}