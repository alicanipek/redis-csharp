using System.Collections.Concurrent;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.Services;

public class ListStorageService
{
    private readonly ConcurrentDictionary<string, BlockingList> _listStore = new();

    public async Task<int> RPushAsync(string key, IEnumerable<string> values)
    {
        var list = _listStore.GetOrAdd(key, _ => new BlockingList());
        await list.RPushAsync(values.ToArray());
        return list._items.Count;
    }

    public async Task<string?> BLPopAsync(string key, int timeout)
    {
        var list = _listStore.GetOrAdd(key, _ => new BlockingList());
        var result = await list.BLPopAsync(timeout);
        return result;
    }

    public async Task<int> LPushAsync(string key, IEnumerable<string> values)
    {
        var list = _listStore.GetOrAdd(key, _ => new BlockingList());
        await list.LPushAsync(values.ToArray());
        return list._items.Count;
    }

    public Task<List<string>> LRangeAsync(string key, int start, int stop)
    {
        if (!_listStore.TryGetValue(key, out var list))
        {
            return Task.FromResult(new List<string>());
        }

        
        if (start < 0) start = list._items.Count + start < 0 ? 0 : list._items.Count + start;
        if (stop < 0) stop = list._items.Count + stop < 0 ? 0 : list._items.Count + stop;

        
        stop = Math.Min(stop, list._items.Count - 1);

        if (start > stop || start >= list._items.Count)
        {
            return Task.FromResult(new List<string>());
        }

        return Task.FromResult(list._items.GetRange(start, stop - start + 1));
    }

    public Task<int> LLenAsync(string key)
    {
        var result = _listStore.TryGetValue(key, out var list) ? list._items.Count : 0;
        return Task.FromResult(result);
    }

    public Task<string?> LPopAsync(string key)
    {
        if (!_listStore.TryGetValue(key, out var list) || list._items.Count == 0)
        {
            return Task.FromResult<string?>(null);
        }

        var popped = list._items[0];
        list._items.RemoveAt(0);
        return Task.FromResult<string?>(popped);
    }

    public Task<List<string>> LPopAsync(string key, int count)
    {
        var result = new List<string>();
        if (!_listStore.TryGetValue(key, out var list))
        {
            return Task.FromResult(result);
        }

        int index = 0;
        while (index < count && list._items.Count > 0)
        {
            var popped = list._items[0];
            list._items.RemoveAt(0);
            result.Add(popped);
            index++;
        }
        return Task.FromResult(result);
    }
}