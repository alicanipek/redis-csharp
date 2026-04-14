using Microsoft.Extensions.DependencyInjection;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Services;
using System.Reflection;
using codecrafters_redis.src.Infrastructure;
using System.CommandLine;
using System.CommandLine.Parsing;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Replica;
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

        var replicaOfOption = new Option<ReplicaInfo>(name: "--replicaof")
        {
            Description = "Address of the primary Redis server to replicate from (format: host port)",
            CustomParser = result =>
            {
                var replicaInfo = result.Tokens.Single().Value.Split(' ');
                if (replicaInfo.Length != 2)
                {
                    result.AddError("--replicaof requires two arguments");
                    return null;
                }
                return new ReplicaInfo
                {
                    Host = replicaInfo[0],
                    Port = int.Parse(replicaInfo[1])
                };
            }
        };

        var dirOption = new Option<string>(name: "--dir")
        {
            Description = "Directory for database files"
        };

        var dbFilenameOption = new Option<string>(name: "--dbfilename")
        {
            Description = "Filename for the database file"
        };

        RootCommand rootCommand = new();
        rootCommand.Options.Add(portOption);
        rootCommand.Options.Add(replicaOfOption);
        rootCommand.Options.Add(dirOption);
        rootCommand.Options.Add(dbFilenameOption);
        ParseResult parseResult = rootCommand.Parse(args);
        foreach (ParseError parseError in parseResult.Errors)
        {
            Console.WriteLine(parseError.Message);
        }

        int port = parseResult.GetValue(portOption);
        ReplicaInfo? replicaInfo = parseResult.GetValue(replicaOfOption);
        var isReplica = replicaInfo != null;
        replicaInfo ??= new ReplicaInfo();

        var dbFileConfig = new DbFileConfig
        {
            Dir = parseResult.GetValue(dirOption),
            DbFilename = parseResult.GetValue(dbFilenameOption)
        };


        var services = new ServiceCollection();
        services.AddSingleton(new Config(port, isReplica, dbFileConfig, replicaInfo));

        // Core services
        services.AddSingleton<IPubSubService, PubSubService>();
        services.AddSingleton<StorageService>();
        services.AddSingleton<ListStorageService>();
        services.AddSingleton<StreamStorageService>();
        services.AddSingleton<SortedSetStorageService>();
        services.AddSingleton<ReplicaManager>();
        services.AddSingleton<UserManager>();
        services.AddSingleton<IWatchedKeysService, WatchedKeysService>();
        services.AddSingleton<CommandProcessor>();


        RegisterCommandHandlers(services);

        services.AddSingleton<RedisServer>();

        var serviceProvider = services.BuildServiceProvider();
        var storageService = serviceProvider.GetRequiredService<StorageService>();
        var path = dbFileConfig.DbFilename != null && dbFileConfig.Dir != null ? Path.Combine(dbFileConfig.Dir, dbFileConfig.DbFilename) : "";
        var initialData = await RDBFileParser.LoadAsync(path);
        if (initialData != null)
        {
            storageService.Init(initialData.Databases[0]);
        }

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