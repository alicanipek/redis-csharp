using System.Net;
using System.Net.Sockets;
using codecrafters_redis.Services;

namespace codecrafters_redis.Infrastructure;

public class RedisServer
{
    private readonly CommandProcessor _commandProcessor;
    private readonly TcpListener _server;

    public RedisServer(CommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
        _server = new TcpListener(IPAddress.Any, 6379);
    }

    public async Task StartAsync()
    {
        _server.Start();

        // Enter the listening loop.
        while (true)
        {
            // Perform a blocking call to accept requests.
            TcpClient client = await _server.AcceptTcpClientAsync();

            _ = Task.Run(async () =>
            {
                using (client)
                {
                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();
                    var buffer = new byte[4096];

                    try
                    {
                        int bytesRead;
                        // Loop to receive all the data sent by the client.
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            System.Console.WriteLine("Received request at time: " + DateTime.Now.ToString("hh:mm:ss.fff"));
                            string request = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            byte[] response = await _commandProcessor.ProcessCommandAsync(request);
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