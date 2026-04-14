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

    public bool Contains(string key)
    {
        return Commands.Any(cmd => cmd.Contains(key));
    }

    public void PrintQueue()
    {
        Console.WriteLine("Current Command Queue:");
        foreach (var cmd in Commands)
        {
            Console.WriteLine(cmd);
        }
    }
}
