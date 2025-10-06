using System;
using System.Text;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class XAddCommandHandler : ICommandHandler
{
    public StreamStorageService _streamStorageService;
    public RespParser _respParser;
    public XAddCommandHandler(StreamStorageService streamStorageService, RespParser respParser)
    {
        _streamStorageService = streamStorageService;
        _respParser = respParser;
    }
    public string CommandName => "XADD";

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 4)
        {
            return System.Text.Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;
        var id = arguments[2].ToString()!;

        var fields = new Dictionary<string, string>();
        for (int i = 3; i < arguments.Count; i += 2)
        {
            if (i + 1 >= arguments.Count)
            {
                return System.Text.Encoding.ASCII.GetBytes("-ERR wrong number of fields\r\n");
            }
            var field = arguments[i].ToString()!;
            var value = arguments[i + 1].ToString()!;
            fields[field] = value;
        }
        try
        {

            await _streamStorageService.AddEntryAsync(key, id, fields);
        }
        catch (ArgumentException ex)
        {
            return Encoding.ASCII.GetBytes($"-ERR {ex.Message}\r\n");
        }

        return Encoding.ASCII.GetBytes(_respParser.EncodeBulkString(id));

    }
}
