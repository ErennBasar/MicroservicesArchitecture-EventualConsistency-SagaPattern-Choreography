using System.Text;
using System.Text.Json;
using EventStore.Client;

namespace Order.API.Services;

public class EventStoreService
{
    private readonly EventStoreClient _client;

    public EventStoreService(EventStoreClient client)
    {
        _client = client;
    }
    // Olayı Veritabanına Ekleme (Append)
    public async Task AppendToStreamAsync(
        string streamName, 
        IEnumerable<object> eventDataList, 
        object? metadata = null
        )
    {
        // Eğer metadata gelmediyse boş bir nesne oluştur, geldiyse JSON yap
        var metadataBytes = metadata != null 
            ? Encoding.UTF8.GetBytes(JsonSerializer.Serialize(metadata)) 
            : Array.Empty<byte>();
        
        var eventData = eventDataList.Select(s => new EventData(
            eventId: Uuid.NewUuid(),
            type:s.GetType().Name, // Class ismini Event Tipi olarak kullanıyoruz (örn: OrderCreatedEvent)
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(s))
        ));

        await _client.AppendToStreamAsync(
            streamName: streamName,
            expectedState:StreamState.Any,
            eventData: eventData
        );
    }

    // Olayları Okuma (Read) 
    public async Task<EventStoreClient.ReadStreamResult> ReadStreamAsync(string streamName)
    {
        return _client.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start
        );
    }
    // Abonelik (Subscribe)
    public async Task SubscribeToStreamAsync(string streamName,
        Func<StreamSubscription, ResolvedEvent, CancellationToken, Task> eventAppeared)
    {
        await _client.SubscribeToStreamAsync(
            streamName: streamName,
            start: FromStream.Start, 
            eventAppeared: eventAppeared,
            resolveLinkTos: true
        );
    }
}