using System.Security.Cryptography;
using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class User
{
    public required string UserName { get; set; }
    public required string[] Passwords { get; set; }
    public required string[] Flags { get; set; }

    public byte[] ToRespArrayBytes()
    {
        return RespParser.EncodeRespArrayBytes(["flags", Flags, "passwords", Passwords]);
    }

}



public class AclCommandHandler : ICommandHandler
{
    public string CommandName => "ACL";
    public bool IsWriteCommand => false;

    public List<User> Users { get; } = new List<User>
    {
        new User
        {
            UserName = "default",
            Passwords = Array.Empty<string>(),
            Flags = ["nopass"]
        }
    };

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
            return Users.First(u => u.UserName == userName).ToRespArrayBytes();
        }
        if (arguments.Count == 4 && arguments[1].ToString()?.ToUpper() == "SETUSER" && arguments[3].ToString()!.StartsWith('<'))
        {
            var userName = arguments[2].ToString()!;
            var user = Users.First(u => u.UserName == userName);
            var password = arguments[3].ToString()![1..];
            var sha256Hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
            user.Passwords = [sha256Hash];
            return RespParser.EncodeSimpleString("OK");
        }
        return RespParser.EncodeErrorString("unsupported ACL subcommand");
    }
}