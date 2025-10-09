using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.Infrastructure;

public class ClientSession
{
    public bool IsMultiActive { get; private set; }
    public CommandQueue CommandQueue { get; }

    public ClientSession()
    {
        IsMultiActive = false;
        CommandQueue = new CommandQueue();
    }

    public bool ToggleMultiActiveState(bool isActive)
    {
        return IsMultiActive = isActive;
    }

}