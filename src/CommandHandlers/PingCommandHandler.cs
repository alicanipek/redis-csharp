using System.Text;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class PingCommandHandler : ICommandHandler
{
    public string CommandName => "PING";
    public bool IsWriteCommand => false; 

    public Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (clientSession != null && clientSession.IsInPubSubMode)
        {
            return Task.FromResult(RespParser.EncodeRespArrayBytes(["pong", ""]));
        }
        return Task.FromResult(RespParser.EncodeSimpleString("PONG"));
    }
}