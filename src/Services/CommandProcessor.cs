using System.Text;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

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

    public async Task<byte[]> ProcessCommandAsync(string request, ClientSession? clientSession)
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

        
        if (commandName == "MULTI")
        {
            if (clientSession != null)
            {
                clientSession.ToggleMultiActiveState(true);
                return Encoding.ASCII.GetBytes("+OK\r\n");
            }
        }

        
        if (commandName == "EXEC")
        {
            if (clientSession != null && clientSession.IsMultiActive)
            {
                return await HandleExecCommand(clientSession);
            }
            return Encoding.ASCII.GetBytes("-ERR EXEC without MULTI\r\n");
        }

        if (commandName == "DISCARD")
        {
            if (clientSession != null && clientSession.IsMultiActive)
            {
                clientSession.ToggleMultiActiveState(false);
                clientSession.CommandQueue.Clear();
                return Encoding.ASCII.GetBytes("+OK\r\n");
            }
            return Encoding.ASCII.GetBytes("-ERR DISCARD without MULTI\r\n");
        }


        if (clientSession != null && clientSession.IsMultiActive)
        {
            clientSession.CommandQueue.Enqueue(request);
            return Encoding.ASCII.GetBytes("+QUEUED\r\n");
        }

        
        var handler = _handlers.FirstOrDefault(h => h.Key.Equals(commandName, StringComparison.OrdinalIgnoreCase)).Value;

        if (handler != null)
        {
            return await handler.HandleAsync(parsed);
        }

        return Encoding.ASCII.GetBytes("-ERR unknown command\r\n");
    }

    private async Task<byte[]> HandleExecCommand(ClientSession clientSession)
    {
        if (!clientSession.IsMultiActive)
        {
            return Encoding.ASCII.GetBytes("-ERR EXEC without MULTI\r\n");
        }


        clientSession.ToggleMultiActiveState(false);

        if (clientSession.CommandQueue.IsEmpty())
        {
            return Encoding.ASCII.GetBytes("*0\r\n");
        }

        var results = new List<byte[]>();
        
        
        while (!clientSession.CommandQueue.IsEmpty())
        {
            var queuedCommand = clientSession.CommandQueue.Dequeue();
            var result = await ProcessCommandAsync(queuedCommand, null);
            results.Add(result);
        }

        
        var response = new StringBuilder();
        response.Append($"*{results.Count}\r\n");
        
        foreach (var result in results)
        {
            response.Append(Encoding.ASCII.GetString(result));
        }

        return Encoding.ASCII.GetBytes(response.ToString());
    }
}