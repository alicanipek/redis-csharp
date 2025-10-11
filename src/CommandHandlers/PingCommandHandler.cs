using System.Text;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class PingCommandHandler : ICommandHandler
{
    public string CommandName => "PING";
    public bool IsWriteCommand => false; 

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(RespParser.EncodeSimpleString("PONG"));
    }
}