using System;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class ExecCommandHandler(Config config, CommandQueue commandQueue) : ICommandHandler
{
    public string CommandName => "EXEC";

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (config.IsMultiActive && commandQueue.Commands.Count == 0)
        {
            config.IsMultiActive = false;
            return Task.FromResult(System.Text.Encoding.ASCII.GetBytes("*0\r\n"));
        }
        return Task.FromResult(System.Text.Encoding.ASCII.GetBytes("-ERR EXEC without MULTI\r\n"));
    }

}
