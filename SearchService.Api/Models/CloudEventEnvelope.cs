namespace SearchService.Api.Models;

public class CloudEventEnvelope<T>
{
    public string Id { get; set; } = default!;
    public string Source { get; set; } = default!;
    public string Specversion { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Datacontenttype { get; set; } = default!;
    public T Data { get; set; } = default!;
}
