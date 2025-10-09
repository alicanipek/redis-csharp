using System;
using codecrafters_redis.CommandHandlers;

namespace codecrafters_redis.src.CommandHandlers;

public class ReplconfCommandHandler : ICommandHandler
{
    public string CommandName => "REPLCONF";

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(System.Text.Encoding.ASCII.GetBytes("+OK\r\n"));
    }
}
