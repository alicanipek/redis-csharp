using System.Text;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;

namespace codecrafters_redis.CommandHandlers;

public class SetCommandHandler : ICommandHandler
{
    private readonly StorageService _storageService;
    private readonly RespParser _respParser;

    public string CommandName => "SET";

    public SetCommandHandler(StorageService storageService, RespParser respParser)
    {
        _storageService = storageService;
        _respParser = respParser;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 3)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;
        var value = arguments[2].ToString()!;
        int? expirationMs = null;

        if (arguments.Count > 3 && arguments[3].ToString()!.ToUpper() == "PX" && arguments.Count > 4)
        {
            expirationMs = int.Parse(arguments[4].ToString()!);
        }

        await _storageService.SetAsync(key, value, expirationMs);
        return Encoding.ASCII.GetBytes("+OK\r\n");
    }
}