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

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count != 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments for 'INCR' command");
        }

        var key = arguments[1].ToString()!;

        try
        {
            var newValue = await storageService.IncrementKeyAsync(key);
            return RespParser.EncodeIntegerBytes(newValue);
        }
        catch (FormatException)
        {
            return RespParser.EncodeErrorString("value is not an integer or out of range");
        }
    }
}
