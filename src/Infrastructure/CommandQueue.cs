using System;
using codecrafters_redis.Services;

namespace codecrafters_redis.src.Infrastructure;

public class CommandQueue
{
    private Queue<string> Commands { get; } = new Queue<string>();
    
    public void Enqueue(string command)
    {
        Commands.Enqueue(command);
    }

    public string Dequeue()
    {
        return Commands.Dequeue();
    }

    public void Clear()
    {
        Commands.Clear();
    }

    public bool IsEmpty()
    {
        return Commands.Count == 0;
    }
}
