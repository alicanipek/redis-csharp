using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class LPushCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;

    public string CommandName => "LPUSH";
    public bool IsWriteCommand => true; 

    public LPushCommandHandler(ListStorageService listService)
    {
        _listService = listService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 3)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var values = arguments.Skip(2).Select(v => v.ToString()!).ToList();

        var count = await _listService.LPushAsync(key, values);

        return RespParser.EncodeIntegerBytes(count);
    }
}