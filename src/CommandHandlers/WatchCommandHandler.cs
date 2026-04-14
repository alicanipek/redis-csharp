using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class WatchCommandHandler : ICommandHandler
{
    public string CommandName => "WATCH";
    public bool IsWriteCommand => false; 

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        return RespParser.OkBytes;
    }
}