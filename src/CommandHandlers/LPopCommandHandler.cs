using System.Text;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;

namespace codecrafters_redis.CommandHandlers;

public class LPopCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;
    private readonly RespParser _respParser;

    public string CommandName => "LPOP";

    public LPopCommandHandler(ListStorageService listService, RespParser respParser)
    {
        _listService = listService;
        _respParser = respParser;
    }

    public byte[] Handle(List<object> arguments)
    {
        if (arguments.Count < 2)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;

        if (arguments.Count > 2)
        {
            var count = int.Parse(arguments[2].ToString()!);
            var poppedItems = _listService.LPop(key, count);
            
            var response = $"*{poppedItems.Count}\r\n";
            foreach (var item in poppedItems)
            {
                response += _respParser.EncodeBulkString(item);
            }
            return Encoding.ASCII.GetBytes(response);
        }
        else
        {
            var popped = _listService.LPop(key);
            return Encoding.ASCII.GetBytes(_respParser.EncodeBulkString(popped));
        }
    }
}