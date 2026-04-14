using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class SubscribeCommandHandler(IPubSubService pubSubService) : ICommandHandler
{
    public string CommandName => "SUBSCRIBE";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count < 2)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }
        if (clientSession == null)
        {
            return Task.FromResult(RespParser.EncodeErrorString("ERR Client is not in a subscribe state"));
        }

        var channel = arguments[1].ToString()!;
        pubSubService.Subscribe(clientSession, channel);
        
        return Task.FromResult(RespParser.EncodeRespArrayBytes(new object[] { "subscribe", channel, clientSession.Subscriptions.Count }));
    }
}
