using System.Security.Cryptography;
using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;



public class AuthCommandHandler(UserManager userManager) : ICommandHandler
{
    public string CommandName => "AUTH";
    public bool IsWriteCommand => false;


    public async Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        if (arguments.Count < 3)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }
        var userName = arguments[1].ToString()!;
        var password = arguments[2].ToString()!;
        var user = userManager.GetUser(userName);
        if (user == null)
        {
            return RespParser.EncodeSimpleErrorString("WRONGPASS", "invalid username or password");
        }
        if (userManager.ValidatePassword(user, password))
        {
            if (clientSession != null)
            {
                clientSession.IsAuthenticated = true;
            }
            return RespParser.EncodeSimpleString("OK");
        }
        return RespParser.EncodeSimpleErrorString("WRONGPASS", "invalid username or password");
    }
}