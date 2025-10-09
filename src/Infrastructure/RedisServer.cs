using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_redis.Services;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.Infrastructure;

public class RedisServer
{
    private readonly CommandProcessor _commandProcessor;
    private readonly TcpListener _server;
    private readonly Config _config;
    private readonly RespParser _respParser;

    public RedisServer(CommandProcessor commandProcessor, Config config, RespParser respParser)
    {
        _commandProcessor = commandProcessor;
        _server = new TcpListener(IPAddress.Any, config.Port);
        _config = config;
        _respParser = respParser;
    }

    public async Task StartAsync()
    {
        _server.Start();
        if (_config.IsReplica && _config.ReplicaInfo != null)
        {
            IPAddress IPAddress;
            if (_config.ReplicaInfo.Host == "localhost")
            {
                var hostName = Dns.GetHostName();
                IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
                // This is the IP address of the local machine
                IPAddress = localhost.AddressList[0];
            }
            else
            {
                IPAddress = IPAddress.Parse(_config.ReplicaInfo.Host);
            }
            IPEndPoint ipEndPoint = new(IPAddress, _config.ReplicaInfo.Port);
            TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ipEndPoint);
            NetworkStream stream = tcpClient.GetStream();
            byte[] buffer = new byte[1024];
            var messageBytes = Encoding.UTF8.GetBytes($"*1\r\n$4\r\nPING\r\n");
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            var received = await stream.ReadAsync(buffer, 0, buffer.Length);
            System.Console.WriteLine("Replica to send \"REPLCONF listening-port 6380\" command");
            messageBytes = Encoding.UTF8.GetBytes($"*3\r\n$8\r\nREPLCONF\r\n$14\r\nlistening-port\r\n$4\r\n6380\r\n");
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            System.Console.WriteLine("Replica to send \"REPLCONF capa psync2\" command");
            messageBytes = Encoding.UTF8.GetBytes($"*3\r\n$8\r\nREPLCONF\r\n$4\r\ncapa\r\n$6\r\npsync2\r\n");
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            received = await stream.ReadAsync(buffer, 0, buffer.Length);
            messageBytes = Encoding.UTF8.GetBytes($"*3\r\n$5\r\nPSYNC\r\n$1\r\n?\r\n$2\r\n-1\r\n");
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }

        while (true)
        {
            TcpClient client = await _server.AcceptTcpClientAsync();

            _ = Task.Run(async () =>
            {
                using (client)
                {

                    var clientSession = new ClientSession();

                    NetworkStream stream = client.GetStream();
                    var buffer = new byte[4096];


                    try
                    {
                        int bytesRead;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            System.Console.WriteLine("Received request at time: " + DateTime.Now.ToString("hh:mm:ss.fff"));
                            string request = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            byte[] response = await _commandProcessor.ProcessCommandAsync(request, clientSession);
                            await stream.WriteAsync(response, 0, response.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Client connection error: {ex.Message}");
                    }
                }
            });

        }
    }
}