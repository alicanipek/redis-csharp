using System.Net.Sockets;

namespace codecrafters_redis.src.Infrastructure;

public class ClientSession
{
    public bool IsMultiActive { get; private set; }
    public CommandQueue CommandQueue { get; }
    public bool IsReplica { get; private set; }
    public NetworkStream? ReplicaStream { get; private set; }

    public ClientSession()
    {
        IsMultiActive = false;
        CommandQueue = new CommandQueue();
        IsReplica = false;
    }

    public bool ToggleMultiActiveState(bool isActive)
    {
        return IsMultiActive = isActive;
    }

    public void MarkAsReplica(NetworkStream stream)
    {
        IsReplica = true;
        ReplicaStream = stream;
    }
}