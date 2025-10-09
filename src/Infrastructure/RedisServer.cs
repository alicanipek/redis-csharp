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

    public RedisServer(CommandProcessor commandProcessor, Config config)
    {
        _commandProcessor = commandProcessor;
        _server = new TcpListener(IPAddress.Any, config.Port);
        _config = config;
    }

    public async Task StartAsync()
    {
        _server.Start();
        if (_config.ReplicaInfo.Host != null)
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
            using Socket client = new(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            await client.ConnectAsync(ipEndPoint);
            var messageBytes = Encoding.UTF8.GetBytes($"*1\r\n$4\r\nPING\r\n");
            _ = await client.SendAsync(messageBytes, SocketFlags.None);
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