using System.Text;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.Services;

public class CommandProcessor
{
    private readonly Dictionary<string, ICommandHandler> _handlers;
    private readonly ReplicaManager _replicaManager;

    public CommandProcessor(IEnumerable<ICommandHandler> commandHandlers, ReplicaManager replicaManager)
    {
        _handlers = commandHandlers.ToDictionary(h => h.CommandName, h => h);
        _replicaManager = replicaManager;
    }

    public async Task<byte[]> ProcessCommandAsync(string request, ClientSession? clientSession)
    {
        var parsed = RespParser.ParseRespArray(request);
        if (parsed.Count == 0)
        {
            return RespParser.EncodeErrorString("empty command");
        }

        var commandName = parsed[0].ToString()?.ToUpper();
        if (string.IsNullOrEmpty(commandName))
        {
            return RespParser.EncodeErrorString("invalid command");
        }

        if (commandName == "MULTI")
        {
            if (clientSession != null)
            {
                clientSession.ToggleMultiActiveState(true);
                return RespParser.OkBytes;
            }
        }

        
        if (commandName == "EXEC")
        {
            if (clientSession != null && clientSession.IsMultiActive)
            {
                return await HandleExecCommand(clientSession);
            }
            return RespParser.EncodeErrorString("EXEC without MULTI");
        }

        if (commandName == "DISCARD")
        {
            if (clientSession != null && clientSession.IsMultiActive)
            {
                clientSession.ToggleMultiActiveState(false);
                clientSession.CommandQueue.Clear();
                return RespParser.OkBytes;
            }
            return RespParser.EncodeErrorString("DISCARD without MULTI");
        }


        if (clientSession != null && clientSession.IsMultiActive)
        {
            clientSession.CommandQueue.Enqueue(request);
            return RespParser.EncodeSimpleString("QUEUED");
        }

        
        var handler = _handlers.FirstOrDefault(h => h.Key.Equals(commandName, StringComparison.OrdinalIgnoreCase)).Value;

        if (handler != null)
        {
            var response = await handler.HandleAsync(parsed);


            if (commandName == "PSYNC" && clientSession != null && clientSession.IsReplica && clientSession.ReplicaStream != null)
            {
                _replicaManager.AddReplica(clientSession.ReplicaStream);
            }


            if (handler.IsWriteCommand && (clientSession == null || !clientSession.IsReplica))
            {
                await _replicaManager.PropagateWriteCommandAsync(request);
            }

            return response;
        }

        return RespParser.EncodeErrorString("unknown command");
    }

    private async Task<byte[]> HandleExecCommand(ClientSession clientSession)
    {
        if (!clientSession.IsMultiActive)
        {
            return RespParser.EncodeErrorString("EXEC without MULTI");
        }


        clientSession.ToggleMultiActiveState(false);

        if (clientSession.CommandQueue.IsEmpty())
        {
            return RespParser.EmptyBulkStringArrayBytes;
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