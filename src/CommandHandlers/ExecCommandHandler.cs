using System;
using codecrafters_redis.CommandHandlers;

namespace codecrafters_redis.src.CommandHandlers;

public class ExecCommandHandler : ICommandHandler
{
    public string CommandName => "EXEC";

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(System.Text.Encoding.ASCII.GetBytes("-ERR EXEC without MULTI\r\n"));
    }

}
