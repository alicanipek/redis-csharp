namespace codecrafters_redis.CommandHandlers;

public interface ICommandHandler
{
    string CommandName { get; }
    Task<byte[]> HandleAsync(List<object> arguments);
}