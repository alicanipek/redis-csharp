using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class GeoAddCommandHandler(SortedSetStorageService sortedSetStorageService) : ICommandHandler
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

        if (!IsValidLatitude(location.Latitude) || !IsValidLongitude(location.Longitude))
        {
            return Task.FromResult(RespParser.EncodeErrorString($"invalid longitude,latitude pair {location.Longitude},{location.Latitude}"));
        }

        var addedCount = sortedSetStorageService.ZAdd(key, new List<SetItem>{ new SetItem(0, location.Member) });
        return Task.FromResult(RespParser.EncodeIntegerBytes(addedCount));

    }

    private bool IsValidLatitude(double latitude)
    {
        return latitude >= -85.05112878 && latitude <= 85.05112878;
    }

    private bool IsValidLongitude(double longitude)
    {
        return longitude >= -180 && longitude <= 180;
    }
}
