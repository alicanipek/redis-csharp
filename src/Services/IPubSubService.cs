using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.Services;

public interface IPubSubService
{
    void Subscribe(ClientSession client, string channel);
    void Unsubscribe(ClientSession client, string channel);
    Task<int> PublishAsync(string channel, string message);
    void CleanupClient(ClientSession client);
    int GetSubscriberCount(string channel);
}