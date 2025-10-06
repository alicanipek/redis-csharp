using System.Text;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;

namespace codecrafters_redis.CommandHandlers;

public class RPushCommandHandler : ICommandHandler
{
    private readonly ListStorageService _listService;

    public string CommandName => "RPUSH";

    public RPushCommandHandler(ListStorageService listService)
    {
        _listService = listService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 3)
        {
            return Encoding.ASCII.GetBytes("-ERR wrong number of arguments\r\n");
        }

        var key = arguments[1].ToString()!;
        var values = arguments.Skip(2).Select(v => v.ToString()!).ToList();

        var count = await _listService.RPushAsync(key, values);
        
        return Encoding.ASCII.GetBytes($":{count}\r\n");
    }
}