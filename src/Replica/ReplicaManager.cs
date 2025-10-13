using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.Replica;

public class ReplicaManager(Config config)
{
    private readonly ConcurrentBag<NetworkStream> _replicaStreams = new();
    public int _inSync = 0;
    private TaskCompletionSource<int>? _waitCompletionSource;
    private int _expectedReplicas;
    private bool _hasPendingWrites = false;

    public void AddReplica(NetworkStream stream)
    {
        if (!config.IsReplica)
        {
            _replicaStreams.Add(stream);
        }
    }

    internal async Task WaitForAcksAsync(int expectedReplicas, CancellationToken token)
    {
        if (_replicaStreams.IsEmpty || expectedReplicas == 0)
        {
            return;
        }

        
        if (!_hasPendingWrites)
        {
            // _inSync = Math.Min(_replicaStreams.Count, expectedReplicas);
            _inSync = _replicaStreams.Count;
            return;
        }



        
        _inSync = 0;
        _expectedReplicas = expectedReplicas;
        _waitCompletionSource = new TaskCompletionSource<int>();

        
        var ackReadTasks = _replicaStreams.Select(async stream =>
        {
            try
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"*3\r\n$8\r\nREPLCONF\r\n$6\r\nGETACK\r\n$1\r\n*\r\n"));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error with GETACK/ACK: {ex.Message}");
            }
        }).ToArray();

        try
        {
            
            await Task.WhenAll(ackReadTasks);
            
            
            using var registration = token.Register(() => _waitCompletionSource?.TrySetCanceled());
            await _waitCompletionSource.Task;
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("Timeout or cancellation occurred while waiting for ACKs");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error waiting for ACKs: {ex.Message}");
        }
        finally
        {
            _waitCompletionSource = null;
            
            _hasPendingWrites = false;
        }
    }



    public async Task PropagateWriteCommandAsync(string command)
    {
        if (config.IsReplica || _replicaStreams.IsEmpty)
        {
            return;
        }

        
        _hasPendingWrites = true;

        var commandBytes = Encoding.ASCII.GetBytes(command);
        var failedStreams = new List<NetworkStream>();


        var tasks = _replicaStreams.Select(async stream =>
        {
            try
            {
                await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                await stream.FlushAsync();
            }
            catch (Exception)
            {
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

    public void IncrementAckCount()
    {
        _inSync++;
        if (_waitCompletionSource != null && _inSync >= _expectedReplicas)
        {
            _waitCompletionSource.TrySetResult(_inSync);
        }
    }

    public int ReplicaCount => _replicaStreams.Count;
}