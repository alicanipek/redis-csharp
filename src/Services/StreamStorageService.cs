using System;

namespace codecrafters_redis.src.Services;

public class Stream
{
    public string Id { get; set; }
    public Dictionary<string, string> Fields { get; set; }
}

public class StreamStorageService
{
    private readonly Dictionary<string, List<Stream>> _streams = new();

    public Task AddEntryAsync(string key, string id, Dictionary<string, string> fields)
    {
        if (!_streams.ContainsKey(key))
        {
            _streams[key] = new List<Stream>();
        }

        var entry = new Stream
        {
            Id = id,
            Fields = fields
        };
        _streams[key].Add(entry);
        return Task.CompletedTask;
    }

    public Task AddEntryAsync(string key, Dictionary<string, string> fields)
    {
        var id = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-0";
        return AddEntryAsync(key, id, fields);
    }

    public Task<List<Stream>?> GetEntriesAsync(string key)
    {
        if (_streams.TryGetValue(key, out var entries))
        {
            return Task.FromResult<List<Stream>?>(entries);
        }
        return Task.FromResult<List<Stream>?>(null);
    }

}
