using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class GeoDistCommandHandler(SortedSetStorageService sortedSetStorageService) : ICommandHandler
{
    public string CommandName => "GEODIST";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        var key = arguments[1].ToString()!;
        var member1 = arguments[2].ToString()!;
        var member2 = arguments[3].ToString()!;

        var geoLocation1 = sortedSetStorageService.ZScore(key, member1);
        var geoLocation2 = sortedSetStorageService.ZScore(key, member2);

        var loc1 = new GeoLocation(geoLocation1!.Value, member1);
        var loc2 = new GeoLocation(geoLocation2!.Value, member2);

        var distance = loc1.GeohashGetDistance(loc2);
        return Task.FromResult(RespParser.EncodeBulkStringBytes(distance.ToString()));
    }
}
