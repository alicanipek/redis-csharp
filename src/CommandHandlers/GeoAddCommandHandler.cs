using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class GeoAddCommandHandler(GeoStorageService geoStorageService) : ICommandHandler
{
    public string CommandName => "GEOADD";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        var key = arguments[1].ToString()!;
        var location = new GeoLocation(
            double.Parse(arguments[2].ToString()!),
            double.Parse(arguments[3].ToString()!),
            arguments[4].ToString()!
        );
        try
        {

            var addedCount = geoStorageService.GeoAdd(key, location);
            return Task.FromResult(RespParser.EncodeIntegerBytes(addedCount));

        }
        catch (System.Exception ex)
        {
            return Task.FromResult(RespParser.EncodeErrorString(ex.Message));
        }
    }
}
