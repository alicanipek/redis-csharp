using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class ZIncrByCommandHandler(SortedSetStorageService sortedSetStorageService) : ICommandHandler
{
    public string CommandName => "ZINCRBY";
    public bool IsWriteCommand => true;

    public Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count != 4)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }

        var key = arguments[1].ToString()!;
        
        if (!double.TryParse(arguments[2].ToString(), out var increment))
        {
            return Task.FromResult(RespParser.EncodeErrorString("ERR value is not a valid float"));
        }
        
        var member = arguments[3].ToString()!;
        
        var newScore = sortedSetStorageService.ZIncrBy(key, member, increment);
        
        return Task.FromResult(RespParser.EncodeBulkStringBytes(newScore.ToString()));
    }
}