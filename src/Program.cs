using Microsoft.Extensions.DependencyInjection;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Services;

class Program
{
    static async Task Main(string[] args)
    {
        // You can use print statements as follows for debugging, they'll be visible when running tests.
        Console.WriteLine("Logs from your program will appear here!");

        // Configure dependency injection
        var services = new ServiceCollection();

        // Register infrastructure services
        services.AddSingleton<RespParser>();

        // Register business services
        services.AddSingleton<StorageService>();
        services.AddSingleton<ListStorageService>();
        services.AddSingleton<StreamStorageService>();
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
        services.AddSingleton<ICommandHandler, TypeCommandHandler>();
        services.AddSingleton<ICommandHandler, XAddCommandHandler>();

        // Register server
        services.AddSingleton<RedisServer>();

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Get the server and start it
        var server = serviceProvider.GetRequiredService<RedisServer>();
        await server.StartAsync();
    }
}