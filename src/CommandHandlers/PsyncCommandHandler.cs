using System;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class PsyncCommandHandler(Config config) : ICommandHandler
{
    public string CommandName => "PSYNC";
    
    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(System.Text.Encoding.ASCII.GetBytes($"+FULLRESYNC {config.ReplicaInfo.Id} {config.ReplicaInfo.Offset}\r\n"));
    }
}
