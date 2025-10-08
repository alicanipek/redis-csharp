using System.Collections.Concurrent;
using codecrafters_redis.Models;

namespace codecrafters_redis.Services;

public class StorageService
{
    private readonly ConcurrentDictionary<string, Item> _store = new();

    public Task SetAsync(string key, string value, int? expirationMs = null)
    {
        var item = new Item
        {
            Value = value,
            Expiration = expirationMs.HasValue ? DateTime.Now.AddMilliseconds(expirationMs.Value) : null
        };
        _store[key] = item;
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key)
    {
        if (_store.TryGetValue(key, out var item))
        {
            if (item.Expiration == null || item.Expiration > DateTime.Now)
            {
                return Task.FromResult<string?>(item.Value);
            }
            else
            {
                _ = _store.TryRemove(key, out var _);
                return Task.FromResult<string?>(null);
            }
        }
        return Task.FromResult<string?>(null);
    }

    internal async Task<int> IncrementKeyAsync(string key)
    {
        var value = await GetAsync(key) ?? throw new FormatException();
        if (!int.TryParse(value, out var intValue))
        {
            throw new FormatException();
        }

        intValue++;
        await SetAsync(key, intValue.ToString());
        return intValue;
    }
}