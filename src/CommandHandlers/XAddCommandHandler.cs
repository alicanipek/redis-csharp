using System;
using System.Text;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class XAddCommandHandler : ICommandHandler
{
    public StreamStorageService _streamStorageService;
    public XAddCommandHandler(StreamStorageService streamStorageService)
    {
        _streamStorageService = streamStorageService;
    }
    public string CommandName => "XADD";
    public bool IsWriteCommand => true; 

    public async Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count < 4)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var id = arguments[2].ToString()!;

        var fields = new Dictionary<string, string>();
        for (int i = 3; i < arguments.Count; i += 2)
        {
            if (i + 1 >= arguments.Count)
            {
                return RespParser.EncodeErrorString("wrong number of fields");
            }
            var field = arguments[i].ToString()!;
            var value = arguments[i + 1].ToString()!;
            fields[field] = value;
        }
        try
        {
            id = await _streamStorageService.AddEntryAsync(key, id, fields);
        }
        catch (ArgumentException ex)
        {
            return RespParser.EncodeErrorString(ex.Message);
        }

        return RespParser.EncodeBulkStringBytes(id);

    }
}
