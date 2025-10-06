using System.Text;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.Infrastructure;

namespace codecrafters_redis.Services;

public class CommandProcessor
{
    private readonly Dictionary<string, ICommandHandler> _handlers;
    private readonly RespParser _respParser;

    public CommandProcessor(IEnumerable<ICommandHandler> commandHandlers, RespParser respParser)
    {
        _respParser = respParser;
        _handlers = commandHandlers.ToDictionary(h => h.CommandName, h => h);
    }

    public async Task<byte[]> ProcessCommandAsync(string request)
    {
        var parsed = _respParser.ParseRespArray(request);
        if (parsed.Count == 0)
        {
            return Encoding.ASCII.GetBytes("-ERR empty command\r\n");
        }

        var commandName = parsed[0].ToString()?.ToUpper();
        if (string.IsNullOrEmpty(commandName))
        {
            return Encoding.ASCII.GetBytes("-ERR invalid command\r\n");
        }

        if (_handlers.TryGetValue(commandName, out var handler))
        {
            return await handler.HandleAsync(parsed);
        }

        return Encoding.ASCII.GetBytes("-ERR unknown command\r\n");
    }
}