using System;
using System.Text;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class ReplconfCommandHandler(Config config) : ICommandHandler
{
    public string CommandName => "REPLCONF";
    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 3)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }

        if (string.Equals(arguments[1].ToString(), "GETACK", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Encoding.ASCII.GetBytes($"*3\r\n$8\r\nREPLCONF\r\n$3\r\nACK\r\n${config.ReplicaInfo.Offset.ToString().Length}\r\n{config.ReplicaInfo.Offset}\r\n"));
        }

        return Task.FromResult(RespParser.OkBytes);
    }
}
