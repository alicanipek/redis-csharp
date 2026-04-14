using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class WatchCommandHandler(IWatchedKeysService watchedKeysService) : ICommandHandler
{
    public string CommandName => "WATCH";
    public bool IsWriteCommand => false; 

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (clientSession == null)
        {
            return RespParser.EncodeErrorString("ERR Client is not in a multi state");
        }

        if (clientSession.IsMultiActive)
        {
            return RespParser.EncodeErrorString("ERR WATCH inside MULTI is not allowed");
        }

        foreach (var arg in arguments.Skip(1))
        {
            watchedKeysService.AddWatchedKey(clientSession.Id, arg.ToString() ?? string.Empty);
        }

        return RespParser.OkBytes;
    }
}