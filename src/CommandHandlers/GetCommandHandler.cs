using System.Text;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;

namespace codecrafters_redis.CommandHandlers;

public class GetCommandHandler : ICommandHandler
{
    private readonly StorageService _storageService;
    private readonly RespParser _respParser;

    public string CommandName => "GET";

    public GetCommandHandler(StorageService storageService, RespParser respParser)
    {
        _storageService = storageService;
        _respParser = respParser;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 2)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;
        var value = await _storageService.GetAsync(key);
        
        return Encoding.ASCII.GetBytes(_respParser.EncodeBulkString(value));
    }
}