using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class WatchCommandHandler : ICommandHandler
{
    public string CommandName => "WATCH";
    public bool IsWriteCommand => false; 

    public async Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (clientSession.IsMultiActive)
        {
            return RespParser.EncodeErrorString("ERR WATCH inside MULTI is not allowed");
        }

        foreach (var arg in arguments.Skip(1))
        {
            if (!_watchedKeys.ContainsKey(clientSession.Id))
            {
                _watchedKeys[clientSession.Id] = new Dictionary<string, bool>();
            }
            _watchedKeys[clientSession.Id][arg.ToString() ?? string.Empty] = false;
        }

        return RespParser.OkBytes;
    }
}