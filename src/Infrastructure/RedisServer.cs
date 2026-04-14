using System.Net;
using System.Net.Sockets;
using codecrafters_redis.src.Replica;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.Infrastructure;

public class RedisServer
{
    private readonly CommandProcessor _commandProcessor;
    private readonly TcpListener _server;
    private readonly Config _config;
    private readonly IPubSubService _pubSubService;
    private readonly UserManager _userManager;
    private readonly Dictionary<int, Dictionary<string, bool>> _watchedKeys = new();

    public RedisServer(CommandProcessor commandProcessor, Config config, IPubSubService pubSubService, UserManager userManager)
    {
        _commandProcessor = commandProcessor;
        _server = new TcpListener(IPAddress.Any, config.Port);
        _config = config;
        _pubSubService = pubSubService;
        _userManager = userManager;
    }

    public async Task StartAsync()
    {
        _server.Start();
        
        if (_config.IsReplica && _config.ReplicaInfo != null)
        {
            var replicaClient = new ReplicaClient(_config.ReplicaInfo, _commandProcessor, _config.Port, _watchedKeys);
            await replicaClient.ConnectToMaster();
        }

        while (true)
        {
            TcpClient client = await _server.AcceptTcpClientAsync();

            _ = Task.Run(async () =>
            {
                using (client)
                {

                    var clientSession = new ClientSession(_userManager);

                    NetworkStream stream = client.GetStream();
                    var buffer = new byte[4096];


                    try
                    {
                        int bytesRead;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            System.Console.WriteLine("Received request at time: " + DateTime.Now.ToString("hh:mm:ss.fff"));
                            string request = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);


                            if (request.ToUpper().Contains("REPLCONF") && request.ToUpper().Contains("LISTENING-PORT"))
                            {
                                clientSession.MarkAsReplica(stream);
                            }
                            
                            if (request.ToUpper().Contains("SUBSCRIBE") || request.ToUpper().Contains("PSUBSCRIBE"))
                            {
                                clientSession.SetStream(stream);
                                clientSession.IsInPubSubMode = true;
                            }
                
                            byte[] response = await _commandProcessor.ProcessCommandAsync(request, clientSession, _watchedKeys);
                            await stream.WriteAsync(response, 0, response.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Client connection error: {ex.Message}");
                    }
                    finally
                    {
                        // Clean up client subscriptions when client disconnects
                        if (clientSession.IsInPubSubMode)
                        {
                            _pubSubService.CleanupClient(clientSession);
                        }
                    }
                }
            });

        }
    }
}