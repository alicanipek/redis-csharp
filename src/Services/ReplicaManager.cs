using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.Services;

public class ReplicaManager(Config config)
{
    private readonly ConcurrentBag<NetworkStream> _replicaStreams = new();

    public void AddReplica(NetworkStream stream)
    {
        if (!config.IsReplica)
        {
            _replicaStreams.Add(stream);
            Console.WriteLine($"Added replica connection. Total replicas: {_replicaStreams.Count}");
        }
    }

    public async Task PropagateWriteCommandAsync(string command)
    {
        if (config.IsReplica || _replicaStreams.IsEmpty)
        {
            return; 
        }

        var commandBytes = Encoding.ASCII.GetBytes(command);
        var failedStreams = new List<NetworkStream>();

        
        var tasks = _replicaStreams.Select(async stream =>
        {
            try
            {
                await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                await stream.FlushAsync();
                Console.WriteLine($"Propagated command to replica: {command.Replace("\r\n", "\\r\\n")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to propagate command to replica: {ex.Message}");
                failedStreams.Add(stream);
            }
        });

        await Task.WhenAll(tasks);

        
        foreach (var failedStream in failedStreams)
        {
            try
            {
                failedStream.Close();
            }
            catch
            {
                System.Console.WriteLine("Failed to close replica stream");
            }
        }
    }

    public int ReplicaCount => _replicaStreams.Count;
}