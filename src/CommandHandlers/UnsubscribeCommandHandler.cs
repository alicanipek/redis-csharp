using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class UnsubscribeCommandHandler(IPubSubService pubSubService) : ICommandHandler
{
    public string CommandName => "UNSUBSCRIBE";
    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count < 2)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }

        if (clientSession == null)
        {
            return Task.FromResult(RespParser.EncodeErrorString("ERR Client session not found"));
        }

        var channel = arguments[1].ToString()!;
        pubSubService.Unsubscribe(clientSession, channel);

        return Task.FromResult(RespParser.EncodeRespArrayBytes(["unsubscribe", channel, clientSession.Subscriptions.Count]));
    }
}
