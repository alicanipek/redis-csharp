using System.Collections.Concurrent;
using codecrafters_redis.src.Models;

namespace codecrafters_redis.src.Services;

public class StorageService
{
    private ConcurrentDictionary<string, Item> _store = new();

    public void Init(Dictionary<string, Item> initialData) => _store = new ConcurrentDictionary<string, Item>(initialData);

    public Task SetAsync(string key, string value, DateTime? expiration = null)
    {
        var item = new Item
        {
            Value = value,
            Expiration = expiration
        };

        _store[key] = item;
        return Task.CompletedTask;
    }

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
        var value = await GetAsync(key);
        int intValue;

        if (value == null)
        {
            intValue = 0;
        }
        else if (!int.TryParse(value, out intValue))
        {
            throw new FormatException();
        }
        intValue++;
        await SetAsync(key, intValue.ToString(), expiration: null);
        return intValue;
    }

    public Task<List<string>> GetAllKeysAsync()
    {
        var keys = _store.Keys.ToList();
        return Task.FromResult(keys);
    }
}