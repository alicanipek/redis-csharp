using System;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class InfoCommandHandler(RespParser respParser) : ICommandHandler
{
    public string CommandName => "INFO";

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        return Task.FromResult(System.Text.Encoding.ASCII.GetBytes(respParser.EncodeBulkString("role:master")));
    }
}
