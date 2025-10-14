using System;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class SubscribeCommandHandler : ICommandHandler
{
    public string CommandName => "SUBSCRIBE";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(RespParser.EncodeRespArrayBytes(new object[] { "subscribe", arguments[1].ToString()!, 1 }));
    }
}
