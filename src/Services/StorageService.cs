using System.Collections.Concurrent;
using codecrafters_redis.Models;

namespace codecrafters_redis.Services;

public class StorageService
{
    private readonly ConcurrentDictionary<string, Item> _store = new();

    public void Set(string key, string value, int? expirationMs = null)
    {
        var item = new Item
        {
            Value = value,
            Expiration = expirationMs.HasValue ? DateTime.Now.AddMilliseconds(expirationMs.Value) : null
        };
        _store[key] = item;
    }

    public string? Get(string key)
    {
        if (_store.TryGetValue(key, out var item))
        {
            if (item.Expiration == null || item.Expiration > DateTime.Now)
            {
                return item.Value;
            }
            else
            {
                _ = _store.TryRemove(key, out var _);
                return null;
            }
        }
        return null;
    }
}