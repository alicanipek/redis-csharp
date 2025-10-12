using System;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class WaitCommandHandler : ICommandHandler
{
    public string CommandName => "WAIT";
    public bool IsWriteCommand => true; 

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(RespParser.EncodeInteger(0));
    }
}
