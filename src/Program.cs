using Microsoft.Extensions.DependencyInjection;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Services;
using System.Reflection;

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

        // Register command handlers automatically using reflection
        RegisterCommandHandlers(services);

        // Register server
        services.AddSingleton<RedisServer>();

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Get the server and start it
        var server = serviceProvider.GetRequiredService<RedisServer>();
        await server.StartAsync();
    }

    private static void RegisterCommandHandlers(IServiceCollection services)
    {
        try
        {
            // Get the current assembly
            var assembly = Assembly.GetExecutingAssembly();

            // Find all types that implement ICommandHandler and are not interfaces or abstract classes
            var handlerTypes = assembly.GetTypes()
                .Where(type => typeof(ICommandHandler).IsAssignableFrom(type) &&
                              !type.IsInterface &&
                              !type.IsAbstract &&
                              type.GetConstructors().Length != 0) // Ensure type has at least one constructor
                .ToList();

            // Register each handler as a singleton
            foreach (var handlerType in handlerTypes)
            {
                services.AddSingleton(typeof(ICommandHandler), handlerType);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during automatic command handler registration: {ex.Message}");
            throw;
        }
    }
}