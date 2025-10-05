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

    public void Start()
    {
        _server.Start();

        // Buffer for reading data
        var bytes = new byte[256];

        // Enter the listening loop.
        while (true)
        {
            // Perform a blocking call to accept requests.
            TcpClient client = _server.AcceptTcpClient();

            _ = Task.Run(() =>
            {
                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    System.Console.WriteLine("Received request at time: " + DateTime.Now.ToString("hh:mm:ss.fff"));
                    string request = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    byte[] response = _commandProcessor.ProcessCommand(request);
                    stream.Write(response, 0, response.Length);
                }
            });
        }
    }
}