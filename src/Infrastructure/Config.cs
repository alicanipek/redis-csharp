using System;

namespace codecrafters_redis.src.Infrastructure;

public class Config
{
    public int Port { get; private set; }
    public Config(int port)
    {
        Port = port;
    }
}
