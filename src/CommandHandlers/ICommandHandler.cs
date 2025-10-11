namespace codecrafters_redis.src.CommandHandlers;

public interface ICommandHandler
{
    string CommandName { get; }
    bool IsWriteCommand { get; } 
    Task<byte[]> HandleAsync(List<object> arguments);
}