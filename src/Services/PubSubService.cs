using System.Collections.Concurrent;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.Services;

public class PubSubService : IPubSubService
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<ClientSession>> _channels = new();
    
    public void Subscribe(ClientSession client, string channel)
    {
        client.Subscriptions.Add(channel);
        client.IsInPubSubMode = true;
        
        _channels.AddOrUpdate(channel, 
            new ConcurrentBag<ClientSession> { client },
            (key, existing) => 
            {
                existing.Add(client);
                return existing;
            });
    }
    
    public void Unsubscribe(ClientSession client, string channel)
    {
        client.Subscriptions.Remove(channel);
        
        if (_channels.TryGetValue(channel, out var subscribers))
        {
            
            var updatedSubscribers = new ConcurrentBag<ClientSession>();
            foreach (var subscriber in subscribers)
            {
                if (subscriber != client && IsClientConnected(subscriber))
                {
                    updatedSubscribers.Add(subscriber);
                }
            }
            
            if (updatedSubscribers.IsEmpty)
            {
                _channels.TryRemove(channel, out _);
            }
            else
            {
                _channels[channel] = updatedSubscribers;
            }
        }
        
        
        if (client.Subscriptions.Count == 0)
        {
            client.IsInPubSubMode = false;
        }
    }
    
    public async Task<int> PublishAsync(string channel, string message)
    {
        if (!_channels.TryGetValue(channel, out var subscribers))
        {
            return 0;
        }
        
        var activeSubscribers = new List<ClientSession>();
        var messagesToSend = new List<Task>();
        
        foreach (var subscriber in subscribers)
        {
            if (IsClientConnected(subscriber))
            {
                activeSubscribers.Add(subscriber);
                messagesToSend.Add(SendMessageToClientAsync(subscriber, channel, message));
            }
        }
        
        
        if (activeSubscribers.Count != subscribers.Count())
        {
            _channels[channel] = new ConcurrentBag<ClientSession>(activeSubscribers);
        }
        
        
        await Task.WhenAll(messagesToSend);
        
        return activeSubscribers.Count;
    }
    
    public void CleanupClient(ClientSession client)
    {
        
        foreach (var channel in client.Subscriptions.ToList())
        {
            Unsubscribe(client, channel);
        }
    }
    
    public int GetSubscriberCount(string channel)
    {
        if (!_channels.TryGetValue(channel, out var subscribers))
        {
            return 0;
        }
        
        return subscribers.Count(IsClientConnected);
    }
    
    private bool IsClientConnected(ClientSession client)
    {
        try
        {
            return client.ClientStream?.CanWrite == true;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task SendMessageToClientAsync(ClientSession client, string channel, string message)
    {
        try
        {
            if (client.ClientStream != null)
            {
                var messageBytes = RespParser.EncodeRespArrayBytes(["message", channel, message]);
                await client.ClientStream.WriteAsync(messageBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to client: {ex.Message}");
            
            
        }
    }
}