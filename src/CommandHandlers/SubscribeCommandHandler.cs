using System;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class SubscribeCommandHandler : ICommandHandler
{
    public string CommandName => "SUBSCRIBE";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count < 2)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }
        if (clientSession == null)
        {
            return Task.FromResult(RespParser.EncodeErrorString("ERR Client is not in a subscribe state"));
        }

        clientSession.IsInPubSubMode = true;
        clientSession.Subscriptions.Add(arguments[1].ToString()!);
        return Task.FromResult(RespParser.EncodeRespArrayBytes(new object[] { "subscribe", arguments[1].ToString()!, clientSession.Subscriptions.Count }));
    }
}
