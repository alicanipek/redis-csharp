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
        System.Console.WriteLine("multi state: " + clientSession?.IsMultiActive); 
        if (clientSession.IsMultiActive)
        {
            return RespParser.EncodeErrorString("ERR WATCH inside MULTI is not allowed");
        }

        foreach (var arg in arguments.Skip(1))
        {
            clientSession.WatchedKeys.Add(arg.ToString());
        }

        return RespParser.OkBytes;
    }
}