using System;
using codecrafters_redis.src.Models;

namespace codecrafters_redis.src.Infrastructure;

public class Config(int port, bool isReplica, DbFileConfig? dbFileConfig = null, ReplicaInfo? replicaInfo = null)
{
    public int Port { get; private set; } = port;
    public bool IsReplica { get; private set; } = isReplica;
    public DbFileConfig? DbFileConfig { get; set; } = dbFileConfig;
    public ReplicaInfo? ReplicaInfo { get; set; } = replicaInfo;
    public Dictionary<string, List<ClientSession>> PubSubChannels { get; private set; } = new();
}
