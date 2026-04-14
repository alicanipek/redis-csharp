using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class ZAddCommandHandler(SortedSetStorageService sortedSetStorageService, IWatchedKeysService watchedKeysService) : ICommandHandler
{
    public string CommandName => "ZADD";
    public bool IsWriteCommand => true;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count < 4 || arguments.Count % 2 != 0)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }

        var key = arguments[1].ToString()!;
        var scoreMembers = new List<SetItem>();
        for (int i = 2; i < arguments.Count; i += 2)
        {
            if (!double.TryParse(arguments[i].ToString(), out var score))
            {
                return Task.FromResult(RespParser.EncodeErrorString("ERR value is not a valid float"));
            }
            var member = arguments[i + 1].ToString()!;
            scoreMembers.Add(new SetItem(score, member));
        }

        var addedCount = sortedSetStorageService.ZAdd(key, scoreMembers);
        watchedKeysService.MarkKeyAsModified(key);
        
        return Task.FromResult(RespParser.EncodeIntegerBytes(addedCount));
    }
}
