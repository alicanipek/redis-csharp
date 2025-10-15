using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class ZScoreCommandHandler(SortedSetStorageService sortedSetStorageService) : ICommandHandler
{
    public string CommandName => "ZSCORE";
    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count != 3)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }

        var key = arguments[1].ToString()!;
        var member = arguments[2].ToString()!;
        
        var score = sortedSetStorageService.ZScore(key, member);
        
        if (score == null)
        {
            return Task.FromResult(RespParser.NullBulkBytes);
        }
        
        return Task.FromResult(RespParser.EncodeBulkStringBytes(score.ToString()));
    }
}