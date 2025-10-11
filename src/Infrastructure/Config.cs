using System;
using codecrafters_redis.src.Models;

namespace codecrafters_redis.src.Infrastructure;

public class Config(int port, bool isReplica, ReplicaInfo? replicaInfo = null)
{
    public int Port { get; private set; } = port;
    public bool IsReplica { get; private set; } = isReplica;
    public ReplicaInfo? ReplicaInfo { get; set; } = replicaInfo;
}
