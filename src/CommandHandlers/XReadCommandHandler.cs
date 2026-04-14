using System;
using System.Text;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class XReadCommandHandler : ICommandHandler
{
    public string CommandName => "XREAD";
    public bool IsWriteCommand => false; 

    private readonly StreamStorageService _streamStorageService;

    public XReadCommandHandler(StreamStorageService streamStorageService)
    {
        _streamStorageService = streamStorageService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        try
        {
            var parsedArgs = ParseArguments(arguments);
            var results = await ProcessStreamsAsync(parsedArgs);
            if (results == null)
            {
                return RespParser.NullArrayBytes;
            }
            if (results.Count == 0)
            {
                return RespParser.EmptyArrayBytes;
            }

            var response = FormatResponse(results);
            return Encoding.ASCII.GetBytes(response);
        }
        catch (ArgumentException ex)
        {
            return RespParser.EncodeErrorString(ex.Message);
        }
    }

    private static XReadArguments ParseArguments(List<object> arguments)
    {
        if (arguments.Count < 4)
        {
            throw new ArgumentException("wrong number of arguments");
        }

        var result = new XReadArguments();
        var argIndex = 1;

        if (arguments.Count > 2 && arguments[1].ToString()!.ToUpper() == "BLOCK")
        {
            if (!decimal.TryParse(arguments[2].ToString(), out var timeout))
            {
                throw new ArgumentException("invalid timeout value");
            }
            result.TimeoutMs = timeout;
            argIndex += 2;
        }

        var streamsIndex = -1;
        for (var i = argIndex; i < arguments.Count; i++)
        {
            if (arguments[i].ToString()!.ToUpper() == "STREAMS")
            {
                streamsIndex = i;
                break;
            }
        }

        if (streamsIndex == -1)
        {
            throw new ArgumentException("STREAMS keyword not found");
        }

        var remainingArgs = arguments.Count - streamsIndex - 1;
        if (remainingArgs % 2 != 0)
        {
            throw new ArgumentException("uneven number of keys and IDs");
        }

        var streamCount = remainingArgs / 2;
        for (var i = 0; i < streamCount; i++)
        {
            var key = arguments[streamsIndex + 1 + i].ToString()!;
            var id = arguments[streamsIndex + 1 + streamCount + i].ToString()!;
            result.StreamRequests.Add(new StreamRequest { Key = key, Id = id });
        }

        return result;
    }

    private async Task<List<StreamResult>?> ProcessStreamsAsync(XReadArguments args)
    {
        List<StreamResult>? results = null;

        foreach (var request in args.StreamRequests)
        {
            var entries = request.Id == "$" 
                ? await _streamStorageService.GetRangeAsync(request.Key, (int)args.TimeoutMs)
                : await _streamStorageService.GetRangeAsync(request.Key, request.Id, (int)args.TimeoutMs);

            if (entries != null && entries.Count > 0)
            {
                if (results == null)
                {
                    results = new List<StreamResult>();
                }
                results.Add(new StreamResult { Key = request.Key, Entries = entries });
            }
        }

        return results;
    }

    private static string FormatResponse(List<StreamResult> results)
    {
        var response = new StringBuilder();
        response.Append($"*{results.Count}\r\n");

        foreach (var result in results)
        {
            response.Append("*2\r\n");
            response.Append($"${result.Key.Length}\r\n{result.Key}\r\n");
            response.Append($"*{result.Entries.Count}\r\n");

            foreach (var entry in result.Entries)
            {
                var entryId = entry.Id.ToString();
                response.Append($"*2\r\n${entryId.Length}\r\n{entryId}\r\n*{entry.Fields.Count * 2}\r\n");

                foreach (var field in entry.Fields)
                {
                    response.Append($"${field.Key.Length}\r\n{field.Key}\r\n");
                    response.Append($"${field.Value.Length}\r\n{field.Value}\r\n");
                }
            }
        }

        return response.ToString();
    }

    private class XReadArguments
    {
        public decimal TimeoutMs { get; set; } = 0;
        public List<StreamRequest> StreamRequests { get; set; } = new();
    }

    private class StreamRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }

    private class StreamResult
    {
        public string Key { get; set; } = string.Empty;
        public List<Models.Stream> Entries { get; set; } = new();
    }
}
