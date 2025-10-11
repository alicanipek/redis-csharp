using System;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class ReplconfCommandHandler : ICommandHandler
{
    public string CommandName => "REPLCONF";
    public bool IsWriteCommand => false; 

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(RespParser.OkBytes);
    }
}
