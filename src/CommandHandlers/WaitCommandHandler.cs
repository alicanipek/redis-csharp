using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Replica;

namespace codecrafters_redis.src.CommandHandlers;

public class WaitCommandHandler(ReplicaManager replicaManager) : ICommandHandler
{
    public string CommandName => "WAIT";
    public bool IsWriteCommand => false; 

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count != 3)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }
        if (!int.TryParse(arguments[1].ToString(), out int numReplicas) || numReplicas < 0)
        {
            return RespParser.EncodeErrorString("invalid number of replicas");
        }
        if (!int.TryParse(arguments[2].ToString(), out int timeout) || timeout < 0)
        {
            return RespParser.EncodeErrorString("invalid timeout");
        }

        using var cts = new CancellationTokenSource(timeout);

        await replicaManager.WaitForAcksAsync(numReplicas, cts.Token);

        return RespParser.EncodeIntegerBytes(replicaManager._inSync);
    }
}
