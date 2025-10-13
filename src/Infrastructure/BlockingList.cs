namespace codecrafters_redis.src.Infrastructure;

public class BlockingList
{
    public readonly List<string> _items = new();
    private readonly Queue<TaskCompletionSource<bool>> _waiters = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<int> LPushAsync(string[] values)
    {
        await _lock.WaitAsync();
        try
        {
            foreach (var value in values)
            {
                _items.Insert(0, value);
            }

            while (_waiters.Count > 0 && _items.Count > 0)
            {
                var waiter = _waiters.Dequeue();
                waiter.TrySetResult(true);
            }

            return _items.Count;
        }
        finally
        {
            _lock.Release();
        }
    }
    public async Task<int> RPushAsync(string[] values)
    {
        await _lock.WaitAsync();
        try
        {
            foreach (var value in values)
            {
                _items.Add(value);
            }

            while (_waiters.Count > 0 && _items.Count > 0)
            {
                var waiter = _waiters.Dequeue();
                waiter.TrySetResult(true);
            }

            return _items.Count;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string?> BLPopAsync(int timeoutMilliseconds)
    {
        await _lock.WaitAsync();
        try
        {
            if (_items.Count > 0)
            {
                var value = _items[0];
                _items.RemoveAt(0);
                return value;
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
                if (_items.Count > 0)
                {
                    var value = _items[0];
                    _items.RemoveAt(0);
                    return value;
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
