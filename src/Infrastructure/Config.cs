using System;

namespace codecrafters_redis.src.Infrastructure;

public class Config
{
    public int Port { get; private set; }
    public ReplicaInfo? ReplicaInfo { get; set; } = null;
    public Config(int port, ReplicaInfo? replicaInfo = null)
    {
        Port = port;
        ReplicaInfo = replicaInfo;
    }
}
