using System.Text;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;

namespace codecrafters_redis.CommandHandlers;

public class BLPopCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;
    private readonly WaitableQueue<string> _waitableQueue;
    private readonly RespParser _respParser;

    public string CommandName => "BLPOP";

    public BLPopCommandHandler(ListStorageService listService, WaitableQueue<string> waitableQueue, RespParser respParser)
    {
        _listService = listService;
        _waitableQueue = waitableQueue;
        _respParser = respParser;
    }

    public byte[] Handle(List<object> arguments)
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

        string? item = _waitableQueue.WaitForItem(timeout == 0.0m ? Timeout.Infinite : (int)(timeout * 1000));
        System.Console.WriteLine("BLPOP returned item: " + item);
        if (item == null)
        {
            return Encoding.ASCII.GetBytes("*-1\r\n");
        }
        else
        {
            _listService.RemoveItem(key, item);
            return Encoding.ASCII.GetBytes($"*2\r\n{_respParser.EncodeBulkString(key)}{_respParser.EncodeBulkString(item)}");
        }
    }
}