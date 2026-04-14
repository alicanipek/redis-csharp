using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class LPopCommandHandler(ListStorageService listService, IWatchedKeysService watchedKeysService) : ICommandHandler
{
    public string CommandName => "LPOP";
    public bool IsWriteCommand => true; 

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count < 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        watchedKeysService.MarkKeyAsModified(key);

        if (arguments.Count > 2)
        {
            var count = int.Parse(arguments[2].ToString()!);
            var poppedItems = await listService.LPopAsync(key, count);

            var response = $"*{poppedItems.Count}\r\n";
            foreach (var item in poppedItems)
            {
                response += RespParser.EncodeBulkString(item);
            }
            return Encoding.ASCII.GetBytes(response);
        }
        else
        {
            var popped = await listService.LPopAsync(key);
            return RespParser.EncodeBulkStringBytes(popped);
        }
    }
}