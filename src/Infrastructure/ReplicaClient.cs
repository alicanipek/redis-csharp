using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.Infrastructure;

public class ReplicaClient(ReplicaInfo info, CommandProcessor commandProcessor, int port)
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
    private readonly byte[] _buffer = new byte[4096];
    private readonly TcpClient _connection = new(info.Host, info.Port);

    public async Task ConnectToMaster()
    {
        await Ping();
        await SendReplconfListeningPort();
        await ConfCapabilities();
        await PSync("?", -1);
        _ = Task.Run(HandlePropagatedCommand());
    }

    private Func<Task?> HandlePropagatedCommand()
    {
        return async () =>
        {
            int bytesRead;

            var stream = _connection.GetStream();
            while ((bytesRead = await stream.ReadAsync(_buffer, 0, _buffer.Length)) != 0)
            {
                string request = System.Text.Encoding.ASCII.GetString(_buffer, 0, bytesRead);
                byte[] response = await commandProcessor.ProcessCommandAsync(request, null);
                var commands = ParseCommands(request);
                foreach (var command in commands)
                {
                    await commandProcessor.ProcessCommandAsync(command, null);
                }
            }
        };
    }

    private async Task Ping()
    {
        await SendAndReceiveCommand(RespParser.EncodeBulkStringArrayBytes(new[] { "PING" }));
    }

    private async Task SendReplconfListeningPort()
    {
        await SendAndReceiveCommand(RespParser.EncodeBulkStringArrayBytes(new[] { "REPLCONF", "listening-port", port.ToString() }));
    }

    private async Task ConfCapabilities()
    {
        await SendAndReceiveCommand(RespParser.EncodeBulkStringArrayBytes(new[] { "REPLCONF", "capa", "psync2" }));
    }

    private async Task PSync(string masterReplicationId, int offset)
    {
        var payload = await SendAndReceiveCommand(RespParser.EncodeBulkStringArrayBytes(new[] { "PSYNC", masterReplicationId, offset.ToString() }));
    }

    private async Task<string> SendAndReceiveCommand(byte[] message)
    {
        if (!_connection.Connected) throw new ChannelClosedException();

        var stream = _connection.GetStream();
        await stream.WriteAsync(message);

        using var cts = new CancellationTokenSource(_timeout);
        _buffer.Initialize();
        int received = await stream.ReadAsync(_buffer, cts.Token);
        var payload = Encoding.UTF8.GetString(_buffer, 0, received);
        return payload;
    }

    private List<string> ParseCommands(string input)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(input)) return result;

        var parts = input.Split('*');
        for (int i = 1; i < parts.Length; i++) // Start from 1 to skip empty part before first *
        {
            if (!string.IsNullOrEmpty(parts[i]))
            {
                result.Add("*" + parts[i]);
            }
        }

        return result;
    }
}
