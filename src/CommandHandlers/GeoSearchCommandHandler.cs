using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class GeoSearchCommandHandler(SortedSetStorageService sortedSetStorageService) : ICommandHandler
{
    public string CommandName => "GEOSEARCH";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        var key = arguments[1].ToString()!;
        var lon = double.Parse(arguments[3].ToString()!);
        var lat = double.Parse(arguments[4].ToString()!);
        var radius = double.Parse(arguments[6].ToString()!);

        var set = sortedSetStorageService.GetSortedSet(key);
        if (set == null)
        {
            return Task.FromResult(RespParser.NullArrayBytes);
        }
        var center = new GeoLocation(lon, lat, "center");
        var results = set
            .Where(x =>
            {
                var loc = new GeoLocation(x.Score, x.Member);
                return center.GeohashGetDistanceIfInRadius(loc, radius) == 1;
            })
            .Select(x => x.Member)
            .ToList();
        return Task.FromResult(RespParser.EncodeRespArrayBytes(results.ToArray()));
    }
}
