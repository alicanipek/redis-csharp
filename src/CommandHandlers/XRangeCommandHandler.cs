using System;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class XRangeCommandHandler : ICommandHandler
{
    public StreamStorageService _streamStorageService;
    public XRangeCommandHandler(StreamStorageService streamStorageService)
    {
        _streamStorageService = streamStorageService;
    }
    public string CommandName => "XRANGE";
    public bool IsWriteCommand => false; 

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count < 4 || arguments.Count > 6)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var start = arguments[2].ToString()!;
        var end = arguments[3].ToString()!;

        var entries = await _streamStorageService.GetRangeAsync(key, start, end);

        if (entries == null || entries.Count == 0)
        {
            return RespParser.EmptyBulkStringArrayBytes;
        }

        var response = new System.Text.StringBuilder();
        response.Append($"*{entries.Count}\r\n");
        foreach (var entry in entries)
        {
            response.Append($"*2\r\n${entry.Id.ToString().Length}\r\n{entry.Id}\r\n*{entry.Fields.Count * 2}\r\n");
            foreach (var field in entry.Fields)
            {
                response.Append($"${field.Key.Length}\r\n{field.Key}\r\n${field.Value.Length}\r\n{field.Value}\r\n");
            }
        }

        return System.Text.Encoding.ASCII.GetBytes(response.ToString());


    }
}
