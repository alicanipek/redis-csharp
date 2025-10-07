using System;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class XReadCommandHandler : ICommandHandler
{
    public string CommandName => "XREAD";

    public StreamStorageService _streamStorageService;
    public XReadCommandHandler(StreamStorageService streamStorageService)
    {
        _streamStorageService = streamStorageService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 4)
        {
            return System.Text.Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }
        var timeoutms = 0.0m;
        int i = 1;
        if (arguments.Count > 2 && arguments[1].ToString()!.ToUpper() == "BLOCK")
        {
            timeoutms = decimal.Parse(arguments[2].ToString()!);
            i += 2;
        }

        List<string> keys = new();
        List<string> ids = new();
        
        while (i < arguments.Count)
        {
            if (arguments[i].ToString()!.ToUpper() == "STREAMS") { i++; continue; }
            if (StreamId.TryParse(arguments[i].ToString()!))
            {
                ids.Add(arguments[i].ToString()!);
            }
            else
            {
                keys.Add(arguments[i].ToString()!);
            }
            i++;
        }

        var response = new System.Text.StringBuilder();

        response.Append($"*{keys.Count}\r\n");

        for (int j = 0; j < keys.Count; j++)
        {
            var entries = await _streamStorageService.GetRangeAsync(keys[j], ids[j], (int)timeoutms);

            if (entries == null)
            {
                return System.Text.Encoding.ASCII.GetBytes("*-1\r\n");
            }

            if (entries.Count == 0)
            {
                return System.Text.Encoding.ASCII.GetBytes("*0\r\n");
            }
            response.Append($"*2\r\n");
            response.Append($"${keys[j].Length}\r\n{keys[j]}\r\n");
            response.Append($"*{entries.Count}\r\n");
            foreach (var entry in entries)
            {
                response.Append($"*2\r\n${entry.Id.ToString().Length}\r\n{entry.Id}\r\n*{entry.Fields.Count * 2}\r\n");
                foreach (var field in entry.Fields)
                {
                    response.Append($"${field.Key.Length}\r\n{field.Key}\r\n${field.Value.Length}\r\n{field.Value}\r\n");
                }
            }
        }

        return System.Text.Encoding.ASCII.GetBytes(response.ToString());
    }
}
