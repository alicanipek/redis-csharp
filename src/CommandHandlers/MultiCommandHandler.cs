using System;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class MultiCommandHandler : ICommandHandler
{
    public string CommandName => "MULTI";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (clientSession != null)
        {
            clientSession.ToggleMultiActiveState(true);
            return Task.FromResult(RespParser.OkBytes);
        }
        return Task.FromResult(RespParser.EncodeErrorString("ERR Client is not in a multi state"));
    }
}
