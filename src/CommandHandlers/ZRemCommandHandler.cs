using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class ZRemCommandHandler(SortedSetStorageService sortedSetStorageService) : ICommandHandler
{
    public string CommandName => "ZREM";
    public bool IsWriteCommand => true;

    public Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count < 3)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }

        var key = arguments[1].ToString()!;
        var members = arguments.Skip(2).Select(arg => arg.ToString()!);
        
        var removedCount = sortedSetStorageService.ZRem(key, members);
        
        return Task.FromResult(RespParser.EncodeIntegerBytes(removedCount));
    }
}