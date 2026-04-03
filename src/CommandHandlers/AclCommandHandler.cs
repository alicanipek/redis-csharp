using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class AclCommandHandler : ICommandHandler
{
    public string CommandName => "ACL";
    public bool IsWriteCommand => false;

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        System.Console.WriteLine("Handling ACL command with arguments: " + string.Join(", ", arguments.Select(a => a.ToString())));
        return RespParser.EncodeBulkStringBytes("default");

    }
}