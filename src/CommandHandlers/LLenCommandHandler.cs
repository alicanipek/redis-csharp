using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class LLenCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;

    public string CommandName => "LLEN";
    public bool IsWriteCommand => false; 

    public LLenCommandHandler(ListStorageService listService)
    {
        _listService = listService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count < 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var count = await _listService.LLenAsync(key);

        return RespParser.EncodeIntegerBytes(count);
    }
}