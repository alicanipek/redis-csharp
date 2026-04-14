using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class LRangeCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;

    public string CommandName => "LRANGE";
    public bool IsWriteCommand => false; 

    public LRangeCommandHandler(ListStorageService listService)
    {
        _listService = listService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count < 4)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var start = int.Parse(arguments[2].ToString()!);
        var stop = int.Parse(arguments[3].ToString()!);

        var range = await _listService.LRangeAsync(key, start, stop);
        
        var response = $"*{range.Count}\r\n";
        foreach (var val in range)
        {
            response += RespParser.EncodeBulkString(val);
        }
        
        return Encoding.ASCII.GetBytes(response);
    }
}