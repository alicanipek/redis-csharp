using System;
using System.Collections.Concurrent;
using codecrafters_redis.src.Models;

namespace codecrafters_redis.src.Infrastructure;

public class BlockingStream
{
    public required StreamId Id { get; set; }
    public Dictionary<string, string> Fields { get; set; } = new();
    private readonly Queue<TaskCompletionSource<bool>> _waiters = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<int> AddFieldsAsync(Dictionary<string, string> values)
    {
        await _lock.WaitAsync();
        try
        {
            foreach (var kvp in values)
            {
                Fields[kvp.Key] = kvp.Value;
            }

            // After adding items, notify any waiters (but don't remove items here)
            while (_waiters.Count > 0)
            {
                var waiter = _waiters.Dequeue();
                waiter.TrySetResult(true); // Signal that items are available
            }

            return Fields.Count;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Dictionary<string, string>?> GetFieldsAsync(int timeoutMilliseconds)
    {
        await _lock.WaitAsync();
        try
        {
            if (Fields.Count > 0)
            {
                return Fields;
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Enqueue(tcs);

            _lock.Release(); // unlock before waiting

            Task delay = timeoutMilliseconds > 0
                ? Task.Delay(TimeSpan.FromMilliseconds(timeoutMilliseconds))
                : Task.Delay(Timeout.InfiniteTimeSpan);

            var finished = await Task.WhenAny(tcs.Task, delay);

            if (finished == delay)
                return null;

            // Re-acquire lock and get the item
            await _lock.WaitAsync();
            try
            {
                if (Fields.Count > 0)
                {
                    return Fields;
                }
                return null;
            }
            finally
            {
                _lock.Release();
            }
        }
        finally
        {
            // Ensure lock is released if not already
            if (_lock.CurrentCount == 0)
                _lock.Release();
        }
    }

}
