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
        if (arguments.Count < 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }
        if (arguments[1].ToString()?.ToUpper() == "WHOAMI")
        {
            return RespParser.EncodeBulkStringBytes("default");
        }
        if (arguments.Count == 3 && arguments[1].ToString()?.ToUpper() == "GETUSER" && arguments[2].ToString() == "default")
        {
            return RespParser.EncodeRespArrayBytes(["flags", new string[] { "nopass" }, "passwords", Array.Empty<string>()]);
        }
        return RespParser.EncodeErrorString("unsupported ACL subcommand");
    }
}