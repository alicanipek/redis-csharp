using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class LPopCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;

    public string CommandName => "LPOP";
    public bool IsWriteCommand => true; 

    public LPopCommandHandler(ListStorageService listService)
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

        if (arguments.Count > 2)
        {
            var count = int.Parse(arguments[2].ToString()!);
            var poppedItems = await _listService.LPopAsync(key, count);

            var response = $"*{poppedItems.Count}\r\n";
            foreach (var item in poppedItems)
            {
                response += RespParser.EncodeBulkString(item);
            }
            return Encoding.ASCII.GetBytes(response);
        }
        else
        {
            var popped = await _listService.LPopAsync(key);
            return RespParser.EncodeBulkStringBytes(popped);
        }
    }
}