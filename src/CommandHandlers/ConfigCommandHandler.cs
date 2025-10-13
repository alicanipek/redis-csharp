using System;
using System.Windows.Input;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class ConfigCommandHandler(Config config) : ICommandHandler
{
    public string CommandName => "CONFIG";
    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 2)
        {
            return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments"));
        }

        if (string.Equals(arguments[1].ToString(), "GET", StringComparison.OrdinalIgnoreCase))
        {
            if (arguments.Count != 3)
            {
                return Task.FromResult(RespParser.EncodeErrorString("wrong number of arguments for 'get'"));
            }

            var parameter = arguments[2].ToString();
            if (string.Equals(parameter, "dir", StringComparison.OrdinalIgnoreCase))
            {
                var dir = config.DbFileConfig?.Dir ?? "";
                return Task.FromResult(RespParser.EncodeBulkStringArrayBytes(new[] { "dir", dir }));
            }
            else if (string.Equals(parameter, "dbfilename", StringComparison.OrdinalIgnoreCase))
            {
                var dbFilename = config.DbFileConfig?.DbFilename ?? "";
                return Task.FromResult(RespParser.EncodeBulkStringArrayBytes(new[] { "dbfilename", dbFilename }));
            }
            else if (string.Equals(parameter, "*", StringComparison.OrdinalIgnoreCase))
            {
                var dir = config.DbFileConfig?.Dir ?? "";
                var dbFilename = config.DbFileConfig?.DbFilename ?? "";
                return Task.FromResult(RespParser.EncodeBulkStringArrayBytes(new[] { "dir", dir, "dbfilename", dbFilename }));
            }
            else
            {
                return Task.FromResult(RespParser.EncodeErrorString("Unsupported CONFIG GET parameter"));
            }
        }
        else
        {
            return Task.FromResult(RespParser.EncodeErrorString("Unsupported CONFIG subcommand"));
        }
    }

}
