using System;
using System.Text;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class TypeCommandHandler : ICommandHandler
{
    public string CommandName => "TYPE";
    public StorageService _storageService;
    public TypeCommandHandler(StorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count != 2)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;
        var value = await _storageService.GetAsync(key);
        if (value == null)
        {
            return Encoding.ASCII.GetBytes($"+none\r\n");
        }

        return Encoding.ASCII.GetBytes($"+{value.GetType().Name.ToLower()}\r\n");
    }

}
