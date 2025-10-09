using System.Text;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;

namespace codecrafters_redis.CommandHandlers;

public class BLPopCommandHandler(ListStorageService listService, RespParser respParser) : ICommandHandler
{
    public string CommandName => "BLPOP";

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 2)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;
        var timeout = 0.0m;
        if (arguments.Count > 2)
        {
            timeout = decimal.Parse(arguments[2].ToString()!);
        }

        var timeoutms = (int)(timeout * 1000);
        var item = await listService.BLPopAsync(key, timeoutms);
        System.Console.WriteLine("BLPOP returned item: " + item);
        if (item == null)
        {
            return Encoding.ASCII.GetBytes("*-1\r\n");
        }
        else
        {
            return Encoding.ASCII.GetBytes($"*2\r\n{respParser.EncodeBulkString(key)}{respParser.EncodeBulkString(item)}");
        }
    }
}