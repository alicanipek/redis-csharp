using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class RPushCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;

    public string CommandName => "RPUSH";
    public bool IsWriteCommand => true; 

    public RPushCommandHandler(ListStorageService listService)
    {
        _listService = listService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count < 3)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var values = arguments.Skip(2).Select(v => v.ToString()!).ToList();

        var count = await _listService.RPushAsync(key, values);

        return RespParser.EncodeIntegerBytes(count);
    }
}