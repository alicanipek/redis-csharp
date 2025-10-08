using System;
using System.Windows.Input;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class MultiCommandHandler(Config config) : ICommandHandler
{
    public string CommandName => "MULTI";

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        config.IsMultiActive = true;
        return Task.FromResult(System.Text.Encoding.ASCII.GetBytes("+OK\r\n"));
    }
}
