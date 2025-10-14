using System;
using System.Text;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Replica;

namespace codecrafters_redis.src.CommandHandlers;

public class ReplconfCommandHandler(Config config, ReplicaManager replicaManager) : ICommandHandler
{
    public string CommandName => "REPLCONF";
    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        System.Console.WriteLine("Handling REPLCONF command");
        System.Console.WriteLine($"Arguments: {string.Join(", ", arguments)}");
        if (arguments.Count < 3)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }

        if (string.Equals(arguments[1].ToString(), "GETACK", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Encoding.ASCII.GetBytes($"*3\r\n$8\r\nREPLCONF\r\n$3\r\nACK\r\n${config.ReplicaInfo.Offset.ToString().Length}\r\n{config.ReplicaInfo.Offset}\r\n"));
        }
        int.TryParse(arguments[2].ToString(), out var offset);
        if (string.Equals(arguments[1].ToString(), "ACK", StringComparison.OrdinalIgnoreCase) && offset >= 0)
        {
            replicaManager.IncrementAckCount();
            return Task.FromResult(Array.Empty<byte>());
        }
        return Task.FromResult(RespParser.OkBytes);
    }
}
