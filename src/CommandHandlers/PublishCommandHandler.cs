using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class PublishCommandHandler(IPubSubService pubSubService) : ICommandHandler
{
    public string CommandName => "PUBLISH";
    public bool IsWriteCommand => false;

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count != 3)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }
        
        var channel = arguments[1].ToString()!;
        var message = arguments[2].ToString()!;
        
        var subscribersCount = await pubSubService.PublishAsync(channel, message);
        return RespParser.EncodeIntegerBytes(subscribersCount);
    }
}
