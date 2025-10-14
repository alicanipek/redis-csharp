using System;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class UnsubscribeCommandHandler(Config config) : ICommandHandler
{
    public string CommandName => "UNSUBSCRIBE";
    public bool IsWriteCommand => false;

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count < 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var channel = arguments[1].ToString()!;
        if (clientSession != null)
        {
            clientSession.Subscriptions.Remove(channel);
        }

        if (config.PubSubChannels.TryGetValue(channel, out var subscribers))
        {
            subscribers.RemoveAll(s => s == clientSession);
            if (subscribers.Count == 0)
            {
                config.PubSubChannels.Remove(channel);
            }
        }

        return RespParser.EncodeRespArrayBytes(["unsubscribe", channel, clientSession?.Subscriptions.Count ?? 0]);
    }
}
