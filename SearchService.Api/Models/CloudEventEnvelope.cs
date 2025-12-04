namespace SearchService.Api.Models;

public class CloudEventEnvelope<T>
{
    public string Id { get; set; }
    public string Source { get; set; }
    public string Specversion { get; set; }
    public string Type { get; set; }
    public string Datacontenttype { get; set; }
    public T Data { get; set; }
}
