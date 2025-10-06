using System.Text;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;

namespace codecrafters_redis.CommandHandlers;

public class LLenCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;
    private readonly RespParser _respParser;

    public string CommandName => "LLEN";

    public LLenCommandHandler(ListStorageService listService, RespParser respParser)
    {
        _listService = listService;
        _respParser = respParser;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 2)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;
        var count = await _listService.LLenAsync(key);
        
        return Encoding.ASCII.GetBytes($":{count}\r\n");
    }
}