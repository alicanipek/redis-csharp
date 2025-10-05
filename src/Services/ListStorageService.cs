using System.Collections.Concurrent;

namespace codecrafters_redis.Services;

public class ListStorageService
{
    private readonly ConcurrentDictionary<string, List<string>> _listStore = new();

    public int RPush(string key, IEnumerable<string> values)
    {
        var list = _listStore.GetOrAdd(key, _ => new List<string>());
        foreach (var value in values)
        {
            list.Add(value);
        }
        return list.Count;
    }

    public int LPush(string key, IEnumerable<string> values)
    {
        var list = _listStore.GetOrAdd(key, _ => new List<string>());
        foreach (var value in values)
        {
            list.Insert(0, value);
        }
        return list.Count;
    }

    public List<string> LRange(string key, int start, int stop)
    {
        if (!_listStore.TryGetValue(key, out var list))
        {
            return new List<string>();
        }

        // Handle negative indices
        if (start < 0) start = list.Count + start < 0 ? 0 : list.Count + start;
        if (stop < 0) stop = list.Count + stop < 0 ? 0 : list.Count + stop;

        // Adjust stop to be inclusive
        stop = Math.Min(stop, list.Count - 1);

        if (start > stop || start >= list.Count)
        {
            return new List<string>();
        }

        return list.GetRange(start, stop - start + 1);
    }

    public int LLen(string key)
    {
        return _listStore.TryGetValue(key, out var list) ? list.Count : 0;
    }

    public string? LPop(string key)
    {
        if (!_listStore.TryGetValue(key, out var list) || list.Count == 0)
        {
            return null;
        }

        var popped = list[0];
        list.RemoveAt(0);
        return popped;
    }

    public List<string> LPop(string key, int count)
    {
        var result = new List<string>();
        if (!_listStore.TryGetValue(key, out var list))
        {
            return result;
        }

        int index = 0;
        while (index < count && list.Count > 0)
        {
            var popped = list[0];
            list.RemoveAt(0);
            result.Add(popped);
            index++;
        }
        return result;
    }

    public bool RemoveItem(string key, string item)
    {
        if (_listStore.TryGetValue(key, out var list))
        {
            return list.Remove(item);
        }
        return false;
    }
}