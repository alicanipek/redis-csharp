namespace codecrafters_redis.Infrastructure;

public class WaitableQueue<T>
{
    private readonly Queue<object> _queue = new Queue<object>();
    private readonly object _lock = new object();
    private readonly ManualResetEvent _itemAddedEvent = new ManualResetEvent(false);

    public void Enqueue(object item)
    {
        lock (_lock)
        {
            _queue.Enqueue(item);
            _itemAddedEvent.Set(); // Signal that an item has been added
        }
    }

    public T WaitForItem(int timeoutMilliseconds)
    {
        System.Console.WriteLine("WaitableQueue waiting for item with timeout: " + timeoutMilliseconds);
        DateTime timeoutTime = timeoutMilliseconds == Timeout.Infinite
        ? DateTime.MaxValue
        : DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);

        while (true)
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    T item = (T)_queue.Dequeue();
                    if (_queue.Count == 0)
                    {
                        _itemAddedEvent.Reset();
                    }
                    System.Console.WriteLine("WaitableQueue returning item: " + item);
                    return item;
                }
            }

            if (timeoutMilliseconds != Timeout.Infinite)
            {
                TimeSpan remainingTime = timeoutTime - DateTime.UtcNow;
                if (remainingTime <= TimeSpan.Zero)
                {
                    System.Console.WriteLine("WaitableQueue timeout expired, returning null");
                    return default(T)!;
                }
            }

            if (timeoutMilliseconds == Timeout.Infinite)
            {
                _itemAddedEvent.WaitOne(Timeout.Infinite);
            }
            else
            {
                TimeSpan remainingTime = timeoutTime - DateTime.UtcNow;
                if (remainingTime <= TimeSpan.Zero)
                {
                    System.Console.WriteLine("WaitableQueue timeout expired, returning null");
                    return default(T)!;
                }

                int waitMilliseconds = (int)Math.Min(remainingTime.TotalMilliseconds, int.MaxValue);
                _itemAddedEvent.WaitOne(waitMilliseconds);
            }
        }
    }
}
