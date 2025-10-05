using Microsoft.Extensions.DependencyInjection;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;
using codecrafters_redis.CommandHandlers;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Configure dependency injection
var services = new ServiceCollection();

// Register infrastructure services
services.AddSingleton<RespParser>();
services.AddSingleton<WaitableQueue<string>>();

// Register business services
services.AddSingleton<StorageService>();
services.AddSingleton<ListStorageService>();
services.AddSingleton<CommandProcessor>();
// Register command handlers
services.AddSingleton<ICommandHandler, PingCommandHandler>();
services.AddSingleton<ICommandHandler, EchoCommandHandler>();
services.AddSingleton<ICommandHandler, SetCommandHandler>();
services.AddSingleton<ICommandHandler, GetCommandHandler>();
services.AddSingleton<ICommandHandler, RPushCommandHandler>();
services.AddSingleton<ICommandHandler, LPushCommandHandler>();
services.AddSingleton<ICommandHandler, LRangeCommandHandler>();
services.AddSingleton<ICommandHandler, LLenCommandHandler>();
services.AddSingleton<ICommandHandler, LPopCommandHandler>();
services.AddSingleton<ICommandHandler, BLPopCommandHandler>();

// Register server
services.AddSingleton<RedisServer>();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get the server and start it
var server = serviceProvider.GetRequiredService<RedisServer>();
server.Start();