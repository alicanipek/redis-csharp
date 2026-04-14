using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public interface ICommandHandler
{
    string CommandName { get; }
    bool IsWriteCommand { get; } 
    Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null);
}