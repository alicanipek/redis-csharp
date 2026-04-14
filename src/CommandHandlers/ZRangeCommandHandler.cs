using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class ZRangeCommandHandler(SortedSetStorageService sortedSetStorageService) : ICommandHandler
{
    public string CommandName => "ZRANGE";
    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count < 4)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }

        var key = arguments[1].ToString()!;
        
        if (!int.TryParse(arguments[2].ToString(), out var start))
        {
            return Task.FromResult(RespParser.EncodeErrorString("ERR value is not an integer or out of range"));
        }
        
        if (!int.TryParse(arguments[3].ToString(), out var stop))
        {
            return Task.FromResult(RespParser.EncodeErrorString("ERR value is not an integer or out of range"));
        }

        // Check for WITHSCORES option
        var withScores = arguments.Count > 4 && 
                        string.Equals(arguments[4].ToString(), "WITHSCORES", StringComparison.OrdinalIgnoreCase);

        var result = sortedSetStorageService.ZRange(key, start, stop, withScores);
        return Task.FromResult(RespParser.EncodeRespArrayBytes(result.ToArray()));
    }
}