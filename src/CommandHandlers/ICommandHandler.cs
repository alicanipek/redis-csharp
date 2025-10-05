namespace codecrafters_redis.CommandHandlers;

public interface ICommandHandler
{
    string CommandName { get; }
    byte[] Handle(List<object> arguments);
}