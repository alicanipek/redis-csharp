namespace codecrafters_redis.src.Services;

public interface IWatchedKeysService
{
    /// <summary>
    /// Adds a key to the watch list for a specific client
    /// </summary>
    void AddWatchedKey(int clientId, string key);

    /// <summary>
    /// Marks a key as modified for all clients watching it
    /// </summary>
    void MarkKeyAsModified(string key);

    /// <summary>
    /// Checks if any of a client's watched keys were modified
    /// </summary>
    bool WereAnyKeysModified(int clientId);

    /// <summary>
    /// Clears all watched keys for a client (called on DISCARD, EXEC, client disconnect)
    /// </summary>
    void ClearClientWatchedKeys(int clientId);
}
