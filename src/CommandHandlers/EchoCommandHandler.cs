using System.Text;
using codecrafters_redis.Infrastructure;

namespace codecrafters_redis.CommandHandlers;

public class EchoCommandHandler : ICommandHandler
{
    private readonly RespParser _respParser;

    public string CommandName => "ECHO";

    public EchoCommandHandler(RespParser respParser)
    {
        _respParser = respParser;
    }

    public byte[] Handle(List<object> arguments)
    {
        if (arguments.Count > 1)
        {
            return Encoding.ASCII.GetBytes(_respParser.EncodeBulkString(arguments[1].ToString()!));
        }
        return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
    }
}