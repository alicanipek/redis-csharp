using System;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class PublishCommandHandler(Config config) : ICommandHandler
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
        var subscribersCount = config.PubSubChannels.TryGetValue(channel, out List<ClientSession>? value) ? value!.Count : 0;
        if (subscribersCount > 0)
        {
            foreach (var subscriber in value!)
            {
                if (subscriber.ReplicaStream != null)
                {
                    System.Console.WriteLine($"Publishing message to channel {channel} for subscriber.");
                    await subscriber.ReplicaStream.WriteAsync(RespParser.EncodeRespArrayBytes(["message", channel, arguments[2].ToString()!]));
                }
            }
        }
        return RespParser.EncodeIntegerBytes(subscribersCount);
    }
}
