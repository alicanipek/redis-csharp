using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class BLPopCommandHandler(ListStorageService listService) : ICommandHandler
{
    public string CommandName => "BLPOP";
    public bool IsWriteCommand => true; 

    public async Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count < 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var timeout = 0.0m;
        if (arguments.Count > 2)
        {
            timeout = decimal.Parse(arguments[2].ToString()!);
        }

        var timeoutms = (int)(timeout * 1000);
        var item = await listService.BLPopAsync(key, timeoutms);
        
        if (item == null)
        {
            return RespParser.NullArrayBytes;
        }
        else
        {
            return Encoding.ASCII.GetBytes($"*2\r\n{RespParser.EncodeBulkString(key)}{RespParser.EncodeBulkString(item)}");
        }
    }
}