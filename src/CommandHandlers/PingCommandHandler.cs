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

    public byte[] Handle(List<object> arguments)
    {
        return Encoding.ASCII.GetBytes("+PONG\r\n");
    }
}