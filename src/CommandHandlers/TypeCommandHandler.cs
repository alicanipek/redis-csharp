using System;
using System.Text;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.Services;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class TypeCommandHandler : ICommandHandler
{
    public string CommandName => "TYPE";
    public StorageService _storageService;
    public StreamStorageService _streamStorageService;
    public TypeCommandHandler(StorageService storageService, StreamStorageService streamStorageService)
    {
        _storageService = storageService;
        _streamStorageService = streamStorageService;
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
            var stream = await _streamStorageService.GetEntriesAsync(key);
            if (stream != null)
            {
                return Encoding.ASCII.GetBytes($"+stream\r\n");
            }
            else
            {
                return Encoding.ASCII.GetBytes($"+none\r\n");
            }
        }

        return Encoding.ASCII.GetBytes($"+{value.GetType().Name.ToLower()}\r\n");
    }

}
