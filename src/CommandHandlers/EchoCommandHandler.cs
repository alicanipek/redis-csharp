using System.Text;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class EchoCommandHandler : ICommandHandler
{
    public string CommandName => "ECHO";
    public bool IsWriteCommand => false; 

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count > 1)
        {
            return Task.FromResult(RespParser.EncodeBulkStringBytes(arguments[1].ToString()!));
        }
        return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
    }
}