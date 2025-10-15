using System.Net.Sockets;

namespace codecrafters_redis.src.Infrastructure;

public class ClientSession
{
    public bool IsMultiActive { get; private set; }
    public CommandQueue CommandQueue { get; }
    public bool IsReplica { get; private set; }
    public NetworkStream? ClientStream { get; private set; }
    public HashSet<string> Subscriptions { get; } = new();
    public bool IsInPubSubMode { get; set; } = false;

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

    public void SetStream(NetworkStream stream)
    {
        ClientStream = stream;
    }
    
    public void MarkAsReplica(NetworkStream stream)
    {
        IsReplica = true;
        ClientStream = stream;
    }
}