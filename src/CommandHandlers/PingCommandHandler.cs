using System.Text;
using codecrafters_redis.Infrastructure;

namespace codecrafters_redis.CommandHandlers;

public class PingCommandHandler : ICommandHandler
{
    private readonly RespParser _respParser;

    public string CommandName => "PING";

    public PingCommandHandler(RespParser respParser)
    {
        _respParser = respParser;
    }

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(Encoding.ASCII.GetBytes("+PONG\r\n"));
    }
}