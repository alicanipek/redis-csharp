using System;
using System.Text;
using System.Windows.Input;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class IncrCommandHandler(StorageService storageService) : ICommandHandler 
{
    public string CommandName => "INCR";
    public bool IsWriteCommand => true; 

    public async Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count != 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments for 'INCR' command");
        }

        var key = arguments[1].ToString()!;

        try
        {
            var newValue = await storageService.IncrementKeyAsync(key);
            if (clientSession != null && _watchedKeys.ContainsKey(clientSession.Id) && _watchedKeys[clientSession.Id].ContainsKey(key))
            {
                _watchedKeys[clientSession.Id][key] = true; 
            }
            return RespParser.EncodeIntegerBytes(newValue);
        }
        catch (FormatException)
        {
            return RespParser.EncodeErrorString("value is not an integer or out of range");
        }
    }
}
