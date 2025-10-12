using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Replica;

namespace codecrafters_redis.src.CommandHandlers;

public class WaitCommandHandler(ReplicaManager replicaManager) : ICommandHandler
{
    public string CommandName => "WAIT";
    public bool IsWriteCommand => true; 

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(RespParser.EncodeInteger(replicaManager.ReplicaCount));
    }
}
