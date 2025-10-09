using System;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class InfoCommandHandler(RespParser respParser, Config config) : ICommandHandler
{
    public string CommandName => "INFO";

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (config.ReplicaInfo != null)
        {
            return Task.FromResult(System.Text.Encoding.ASCII.GetBytes(respParser.EncodeBulkString($"role:slave")));
        }
        return Task.FromResult(System.Text.Encoding.ASCII.GetBytes(respParser.EncodeBulkString("role:master")));
    }
}
