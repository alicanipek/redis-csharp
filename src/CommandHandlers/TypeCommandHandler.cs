using System;
using System.Text;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class TypeCommandHandler : ICommandHandler
{
    public string CommandName => "TYPE";
    public bool IsWriteCommand => false; 
    public StorageService _storageService;
    public StreamStorageService _streamStorageService;
    public TypeCommandHandler(StorageService storageService, StreamStorageService streamStorageService)
    {
        _storageService = storageService;
        _streamStorageService = streamStorageService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count != 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var value = await _storageService.GetAsync(key);
        if (value == null)
        {
            var stream = await _streamStorageService.GetEntriesAsync(key);
            if (stream != null)
            {
                return RespParser.EncodeSimpleString("stream");
            }
            else
            {
                return RespParser.EncodeSimpleString("none");
            }
        }

        return RespParser.EncodeSimpleString(value.GetType().Name.ToLower());
    }

}
