using System.Security.Cryptography;
using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;



public class AclCommandHandler(UserManager userManager) : ICommandHandler
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
        if (arguments.Count == 3 && arguments[1].ToString()?.ToUpper() == "GETUSER")
        {
            var userName = arguments[2].ToString()!;
            return userManager.GetUser(userName)?.ToRespArrayBytes() ?? RespParser.EncodeErrorString("user not found");
        }
        if (arguments.Count == 4 && arguments[1].ToString()?.ToUpper() == "SETUSER" && arguments[3].ToString()!.StartsWith('>'))
        {
            var userName = arguments[2].ToString()!;
            var user = userManager.GetUser(userName);
            var password = arguments[3].ToString()![1..];
            user.SetPassword(password);
            return RespParser.EncodeSimpleString("OK");
        }
        return RespParser.EncodeErrorString("unsupported ACL subcommand");
    }
}