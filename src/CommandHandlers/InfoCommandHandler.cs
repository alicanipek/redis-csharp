using System;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class InfoCommandHandler(Config config) : ICommandHandler
{
    public string CommandName => "INFO";
    public bool IsWriteCommand => false; 

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (config.IsReplica)
        {
            return Task.FromResult(RespParser.EncodeBulkStringBytes($"role:slave"));
        }
        return Task.FromResult(RespParser.EncodeBulkStringBytes(
        [
            "role:master",
            $"master_replid:{config.ReplicaInfo.Id}",
            $"master_repl_offset:{config.ReplicaInfo.Offset}"
        ]));
    }
}
