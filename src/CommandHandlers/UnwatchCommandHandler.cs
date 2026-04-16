using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class UnwatchCommandHandler(IWatchedKeysService watchedKeysService) : ICommandHandler
{
    public string CommandName => "UNWATCH";
    public bool IsWriteCommand => false;

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (clientSession == null)
        {
            return RespParser.EncodeErrorString("ERR Client is not in a multi state");
        }
        
        watchedKeysService.RemoveWatchedKeys(clientSession.Id);

        return RespParser.OkBytes;
    }
}