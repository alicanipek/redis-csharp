using System.Text;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;

namespace codecrafters_redis.CommandHandlers;

public class LRangeCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;
    private readonly RespParser _respParser;

    public string CommandName => "LRANGE";

    public LRangeCommandHandler(ListStorageService listService, RespParser respParser)
    {
        _listService = listService;
        _respParser = respParser;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 4)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;
        var start = int.Parse(arguments[2].ToString()!);
        var stop = int.Parse(arguments[3].ToString()!);

        var range = await _listService.LRangeAsync(key, start, stop);
        
        var response = $"*{range.Count}\r\n";
        foreach (var val in range)
        {
            response += _respParser.EncodeBulkString(val);
        }
        
        return Encoding.ASCII.GetBytes(response);
    }
}