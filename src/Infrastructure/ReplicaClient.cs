using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using codecrafters_redis.src.Models;

namespace codecrafters_redis.src.Infrastructure;

public class ReplicaClient(ReplicaInfo info, int port)
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
    private readonly byte[] _buffer = new byte[1024];
    private readonly TcpClient _connection = new(info.Host, info.Port);

    public async Task ConnectToMaster()
    {
        await Ping();
        await SendReplconfListeningPort();
        await ConfCapabilities();
        var psyncResponse = await PSync("?", -1);
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

    private async Task<string> PSync(string masterReplicationId, int offset)
    {
        return await SendAndReceiveCommand(RespParser.EncodeBulkStringArrayBytes(new[] { "PSYNC", masterReplicationId, offset.ToString() }));
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
}
