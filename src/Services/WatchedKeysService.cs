namespace codecrafters_redis.src.Services;

public class WatchedKeysService : IWatchedKeysService
{
    private readonly Dictionary<int, Dictionary<string, bool>> _watchedKeys = new();
    private readonly object _lock = new object();

    public void AddWatchedKey(int clientId, string key)
    {
        lock (_lock)
        {
            if (!_watchedKeys.ContainsKey(clientId))
            {
                _watchedKeys[clientId] = new Dictionary<string, bool>();
            }
            _watchedKeys[clientId][key] = false;
        }
    }

    public void MarkKeyAsModified(string key)
    {
        lock (_lock)
        {
            foreach (var keys in _watchedKeys.Values)
            {
                if (keys.ContainsKey(key))
                {
                    keys[key] = true;
                }
            }
        }
    }

    public bool WereAnyKeysModified(int clientId)
    {
        lock (_lock)
        {
            if (!_watchedKeys.ContainsKey(clientId))
            {
                return false;
            }
            return _watchedKeys[clientId].Values.Any(v => v);
        }
    }

    public void ClearClientWatchedKeys(int clientId)
    {
        lock (_lock)
        {
            _watchedKeys.Remove(clientId);
        }
    }

    public void RemoveWatchedKeys(int clientId)
    {
        lock (_lock)
        {
            _watchedKeys.Remove(clientId);
        }
    }
}
