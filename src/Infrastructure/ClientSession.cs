using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.Infrastructure;

public class ClientSession
{
    public Config Config { get; }
    public CommandQueue CommandQueue { get; }

    public ClientSession()
    {
        Config = new Config();
        CommandQueue = new CommandQueue();
    }
}