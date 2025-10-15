using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;
namespace codecrafters_redis.src.CommandHandlers;
public class ZRankCommandHandler(SortedSetStorageService sortedSetStorageService) : ICommandHandler
{
    public string CommandName => "ZRANK";
    public bool IsWriteCommand => false;
    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count != 3)
        {
            throw new ArgumentException("Invalid number of arguments");
        }
        var key = arguments[1].ToString();
        var member = arguments[2].ToString();
        var rank = sortedSetStorageService.ZRank(key, member);
        if (rank == -1)
        {
            return Task.FromResult(RespParser.NullBulkStringBytes);
        }
        return Task.FromResult(RespParser.EncodeIntegerBytes(rank));
    }
}
