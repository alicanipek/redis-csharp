using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class SetCommandHandler(StorageService storageService, IWatchedKeysService watchedKeysService) : ICommandHandler
{
    public string CommandName => "SET";
    public bool IsWriteCommand => true; 

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (arguments.Count < 3)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var value = arguments[2].ToString()!;
        int? expirationMs = null;

        if (arguments.Count > 3 && arguments[3].ToString()!.ToUpper() == "PX" && arguments.Count > 4)
        {
            expirationMs = int.Parse(arguments[4].ToString()!);
        }

        watchedKeysService.MarkKeyAsModified(key);
        await storageService.SetAsync(key, value, expirationMs);
        return RespParser.OkBytes;
    }
}