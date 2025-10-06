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

        ValidateId(id, _streams[key].Count > 0 ? _streams[key].Last().Id : "0-0");

        var entry = new Stream
        {
            Id = id,
            Fields = fields
        };
        _streams[key].Add(entry);
        return Task.CompletedTask;
    }

    private void ValidateId(string id, string v)
    {
        if (id == "0-0")
        {
            throw new ArgumentException($"The ID specified in XADD must be greater than 0-0");
        }
        var parts = id.Split('-');
        if (parts.Length != 2 || !long.TryParse(parts[0], out var ms) || !int.TryParse(parts[1], out var seq))
        {
            throw new ArgumentException("Invalid ID format");
        }
        var lastParts = v.Split('-');
        var lastMs = long.Parse(lastParts[0]);
        var lastSeq = int.Parse(lastParts[1]);

        if (ms < lastMs || (ms == lastMs && seq <= lastSeq))
        {
            var error = v == "0-0" ? $"must be greater than {v}" : "is equal or smaller than the target stream top item";
            throw new ArgumentException($"The ID specified in XADD {error}");
        }
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
