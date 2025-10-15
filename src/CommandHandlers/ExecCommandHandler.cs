using System;
using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;
using Microsoft.Extensions.DependencyInjection;

namespace codecrafters_redis.src.CommandHandlers;

public class ExecCommandHandler(IServiceProvider serviceProvider) : ICommandHandler
{
    public string CommandName => "EXEC";

    public bool IsWriteCommand => false;

    public async Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        if (clientSession == null)
        {
            return RespParser.EncodeErrorString("ERR Client is not in a multi state");
        }

        if (!clientSession.IsMultiActive)
        {
            return RespParser.EncodeErrorString("EXEC without MULTI");
        }


        clientSession.ToggleMultiActiveState(false);

        if (clientSession.CommandQueue.IsEmpty())
        {
            return RespParser.EmptyArrayBytes;
        }

        var results = new List<byte[]>();
        
        
        var commandProcessor = serviceProvider.GetRequiredService<CommandProcessor>();
        
        while (!clientSession.CommandQueue.IsEmpty())
        {
            var queuedCommand = clientSession.CommandQueue.Dequeue();
            var result = await commandProcessor.ProcessCommandAsync(queuedCommand, null);
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
