using System;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class GeoPosCommandHandler(SortedSetStorageService sortedSetStorageService) : ICommandHandler
{
    public string CommandName => "GEOPOS";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
        var key = arguments[1].ToString()!;
        List<string> members = arguments.Skip(2).Select(arg => arg.ToString()!).ToList();
        List<object> results = new List<object>();
        foreach (var member in members)
        {
            var score = sortedSetStorageService.ZScore(key, member);
            if (score == null)
            {
                results.Add(null);
                continue;
            }
            var geoLocation = new GeoLocation((double)score, member);
            results.Add(new []{geoLocation.Longitude.ToString(), geoLocation.Latitude.ToString()});
        }
        return Task.FromResult(RespParser.EncodeRespArrayBytes(results.ToArray()));
    }
}
