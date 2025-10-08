using System;
using codecrafters_redis.Services;

namespace codecrafters_redis.src.Infrastructure;

public class CommandQueue ()
{
    public Queue<string> Commands { get; } = new Queue<string>();
}
