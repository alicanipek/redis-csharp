using System.Text;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Replica;

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


        if (commandName != "EXEC" && commandName != "DISCARD" && clientSession != null && clientSession.IsMultiActive)
        {
            clientSession.CommandQueue.Enqueue(request);
            return RespParser.EncodeSimpleString("QUEUED");
        }

        
        var handler = _handlers.FirstOrDefault(h => h.Key.Equals(commandName, StringComparison.OrdinalIgnoreCase)).Value;

        if (handler != null)
        {
            var response = await handler.HandleAsync(parsed, clientSession);

            if (handler.IsWriteCommand && (clientSession == null || !clientSession.IsReplica))
            {
                await _replicaManager.PropagateWriteCommandAsync(request);
            }

            return response;
        }

        return RespParser.EncodeErrorString("unknown command");
    }

}