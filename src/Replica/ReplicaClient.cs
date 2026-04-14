using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.Replica;

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
                var commands = ParseCommands(request);
                foreach (var command in commands)
                {
                    var response = await commandProcessor.ProcessCommandAsync(command, null);

                    info.Offset += Encoding.ASCII.GetByteCount(command);
                    if (command.Contains("GETACK", StringComparison.OrdinalIgnoreCase))
                    {
                        await stream.WriteAsync(response);
                    }

                }
            }
        };
    }

    private async Task Ping()
    {
        await SendAndReceiveCommand(RespParser.EncodeRespArrayBytes(new[] { "PING" }));
    }

    private async Task SendReplconfListeningPort()
    {
        await SendAndReceiveCommand(RespParser.EncodeRespArrayBytes(new[] { "REPLCONF", "listening-port", port.ToString() }));
    }

    private async Task ConfCapabilities()
    {
        await SendAndReceiveCommand(RespParser.EncodeRespArrayBytes(new[] { "REPLCONF", "capa", "psync2" }));
    }

    private async Task PSync(string masterReplicationId, int offset)
    {
        var payload = await SendAndReceiveCommand(RespParser.EncodeRespArrayBytes(new[] { "PSYNC", masterReplicationId, offset.ToString() }));
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

        int pos = 0;
        while (pos < input.Length)
        {

            while (pos < input.Length && input[pos] != '*')
                pos++;

            if (pos >= input.Length) break;


            int commandEnd = FindCompleteCommand(input, pos);
            if (commandEnd == -1)
            {

                break;
            }


            string command = input.Substring(pos, commandEnd - pos);
            result.Add(command);
            pos = commandEnd;
        }

        return result;
    }

    private int FindCompleteCommand(string input, int start)
    {
        if (start >= input.Length || input[start] != '*') return -1;


        int crlfPos = input.IndexOf("\r\n", start);
        if (crlfPos == -1) return -1;

        if (!int.TryParse(input.Substring(start + 1, crlfPos - start - 1), out int arrayLength))
            return -1;

        int pos = crlfPos + 2;


        for (int i = 0; i < arrayLength; i++)
        {
            if (pos >= input.Length || input[pos] != '$') return -1;


            crlfPos = input.IndexOf("\r\n", pos);
            if (crlfPos == -1) return -1;

            if (!int.TryParse(input.Substring(pos + 1, crlfPos - pos - 1), out int bulkLength))
                return -1;

            pos = crlfPos + 2;


            pos += bulkLength + 2;

            if (pos > input.Length) return -1;
        }

        return pos;
    }
}
