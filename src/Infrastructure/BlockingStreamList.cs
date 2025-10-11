using System;
using codecrafters_redis.src.Models;
using Stream = codecrafters_redis.src.Models.Stream;

namespace codecrafters_redis.src.Infrastructure;

public class BlockingStreamList
{
    public readonly List<Stream> items = new();
    private readonly Queue<TaskCompletionSource<bool>> _waiters = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task AddAsync(Stream value)
    {
        await _lock.WaitAsync();
        try
        {
            items.Add(value);


            while (_waiters.Count > 0 && items.Count > 0)
            {
                var waiter = _waiters.Dequeue();
                waiter.TrySetResult(true); 
            }
            return;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<int> AddRangeAsync(Stream[] values)
    {
        await _lock.WaitAsync();
        try
        {
            foreach (var value in values)
            {
                items.Add(value);
            }


            while (_waiters.Count > 0 && items.Count > 0)
            {
                var waiter = _waiters.Dequeue();
                waiter.TrySetResult(true); 
            }

            return items.Count;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<Stream>?> GetStreamsAsync(int timeoutMilliseconds)
    {
        var lastId = items.Count > 0 ? items[^1].Id : new StreamId(0, 0);
        return await GetStreamsAsync(lastId, timeoutMilliseconds);
    }
    public async Task<List<Stream>?> GetStreamsAsync(StreamId id, int timeoutMilliseconds)
    {
        await _lock.WaitAsync();
        try
        {
            var _items = items.Where(s => s.Id > id).ToList();
            if (_items.Count > 0)
            {
                return _items;
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Enqueue(tcs);

            _lock.Release(); 

            Task delay = timeoutMilliseconds > 0
                ? Task.Delay(TimeSpan.FromMilliseconds(timeoutMilliseconds))
                : Task.Delay(Timeout.InfiniteTimeSpan);

            var finished = await Task.WhenAny(tcs.Task, delay);

            if (finished == delay)
                return null;


            await _lock.WaitAsync();
            try
            {
                _items = items.Where(s => s.Id > id).ToList();
                if (_items.Count > 0)
                {
                    return _items;
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

            if (_lock.CurrentCount == 0)
                _lock.Release();
        }
    }
}
