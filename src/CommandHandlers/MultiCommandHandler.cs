using System;
using System.Windows.Input;
using codecrafters_redis.CommandHandlers;

namespace codecrafters_redis.src.CommandHandlers;

public class MultiCommandHandler : ICommandHandler
{
    public string CommandName => "MULTI";

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(System.Text.Encoding.ASCII.GetBytes("+OK\r\n"));
    }
}
