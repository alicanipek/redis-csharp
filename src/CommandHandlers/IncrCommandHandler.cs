using System;
using System.Text;
using System.Windows.Input;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class IncrCommandHandler(StorageService storageService) : ICommandHandler 
{
    public string CommandName => "INCR";

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count != 2)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments for 'INCR' command\r\n");
        }

        var key = arguments[1].ToString()!;

        try
        {
            var newValue = await storageService.IncrementKeyAsync(key);
            return Encoding.ASCII.GetBytes($":{newValue}\r\n");
        }
        catch (FormatException)
        {
            return Encoding.ASCII.GetBytes("-ERR value is not an integer or out of range\r\n");
        }
    }
}
