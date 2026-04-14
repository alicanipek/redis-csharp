using System.Net.Sockets;

namespace codecrafters_redis.src.Infrastructure;

public class ClientSession
{
    public int Id { get; } = Interlocked.Increment(ref _idCounter);
    private static int _idCounter = 0;
    public bool IsMultiActive { get; private set; }
    public CommandQueue CommandQueue { get; }
    public bool IsReplica { get; private set; }
    public NetworkStream? ClientStream { get; private set; }
    public HashSet<string> Subscriptions { get; } = new();
    public bool IsInPubSubMode { get; set; } = false;
    public bool IsAuthenticated { get; set; }

    public ClientSession(UserManager? userManager = null)
    {
        IsMultiActive = false;
        CommandQueue = new CommandQueue();
        IsReplica = false;
        
        if (userManager != null)
        {
            var defaultUser = userManager.GetUser("default");
            IsAuthenticated = defaultUser?.HasNoPass ?? false;
        }
        else
        {
            IsAuthenticated = false;
        }
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