using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class MultiCommandHandler : ICommandHandler
{
    public string CommandName => "MULTI";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (clientSession != null)
        {
            clientSession.ToggleMultiActiveState(true);
            // DO NOT clear watched keys - they remain active during MULTI/EXEC
            return Task.FromResult(RespParser.OkBytes);
        }
        return Task.FromResult(RespParser.EncodeErrorString("ERR Client is not in a multi state"));
    }
}
