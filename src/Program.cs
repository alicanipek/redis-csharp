using Microsoft.Extensions.DependencyInjection;
using codecrafters_redis.Infrastructure;
using codecrafters_redis.Services;
using codecrafters_redis.CommandHandlers;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Services;
using System.Reflection;
using codecrafters_redis.src.Infrastructure;
using System.CommandLine;
using System.CommandLine.Parsing;
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Logs from your program will appear here!");
        var portOption = new Option<int>(name: "--port")
        {
            Description = "Port number for the Redis server",
            DefaultValueFactory = parseResult => 6379,
        };
        RootCommand rootCommand = new();
        rootCommand.Options.Add(portOption);
        ParseResult parseResult = rootCommand.Parse(args);
        foreach (ParseError parseError in parseResult.Errors)
        {
            Console.WriteLine(parseError.Message);
        }

        int port = parseResult.GetValue(portOption);
        
        var services = new ServiceCollection();


        services.AddSingleton<RespParser>();
        services.AddSingleton(new Config(port));


        services.AddSingleton<StorageService>();
        services.AddSingleton<ListStorageService>();
        services.AddSingleton<StreamStorageService>();
        services.AddSingleton<CommandProcessor>();


        RegisterCommandHandlers(services);

        services.AddSingleton<RedisServer>();

        var serviceProvider = services.BuildServiceProvider();

        var server = serviceProvider.GetRequiredService<RedisServer>();
        await server.StartAsync();
    }

    private static void RegisterCommandHandlers(IServiceCollection services)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();

            var handlerTypes = assembly.GetTypes()
                .Where(type => typeof(ICommandHandler).IsAssignableFrom(type) &&
                              !type.IsInterface &&
                              !type.IsAbstract &&
                              type.GetConstructors().Length != 0)
                .ToList();

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