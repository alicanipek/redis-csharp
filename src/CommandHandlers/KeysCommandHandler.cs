using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class KeysCommandHandler(StorageService storageService) : ICommandHandler
{
    public string CommandName => "KEYS";

    public bool IsWriteCommand => false;

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count != 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var pattern = arguments[1].ToString();
        if (pattern == "*")
        {
            var keys = await storageService.GetAllKeysAsync();
            return RespParser.EncodeBulkStringArrayBytes(keys.ToArray());
        }
        else
        {
            return RespParser.EncodeErrorString("Only '*' pattern is supported");
        }
        
    }
}
