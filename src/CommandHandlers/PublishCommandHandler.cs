using System;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class PublishCommandHandler(Config config) : ICommandHandler
{
    public string CommandName => "PUBLISH";
    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count != 3)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }
        var channel = arguments[1].ToString()!;
        var subscribersCount = config.PubSubChannels.TryGetValue(channel, out List<ClientSession>? value) ? value!.Count : 0;
        
        return Task.FromResult(RespParser.EncodeIntegerBytes(subscribersCount));
    }
}
