namespace codecrafters_redis.src.Models;

public class Stream
{
    public required StreamId Id { get; set; }
    public required Dictionary<string, string> Fields { get; set; }
}
