using System.Text;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;

namespace codecrafters_redis.CommandHandlers;

public class RPushCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;
    private readonly WaitableQueue<string> _waitableQueue;
    private readonly RespParser _respParser;

    public string CommandName => "RPUSH";

    public RPushCommandHandler(ListStorageService listService, WaitableQueue<string> waitableQueue, RespParser respParser)
    {
        _listService = listService;
        _waitableQueue = waitableQueue;
        _respParser = respParser;
    }

    public byte[] Handle(List<object> arguments)
    {
        if (arguments.Count < 3)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;
        var values = arguments.Skip(2).Select(v => v.ToString()!).ToList();

        var count = _listService.RPush(key, values);
        _waitableQueue.Enqueue(values[0]);
        
        return Encoding.ASCII.GetBytes($":{count}\r\n");
    }
}