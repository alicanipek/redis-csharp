using System;
using System.Collections.Concurrent;

namespace codecrafters_redis.src.Services;

public class GeoLocation
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string Member { get; set; }

    public GeoLocation(double longitude, double latitude, string member)
    {
        Longitude = longitude;
        Latitude = latitude;
        Member = member;
    }
}

public class GeoStorageService
{
    private readonly ConcurrentDictionary<string, SortedSet<GeoLocation>> GeoLocations = new();

    public int GeoAdd(string key, GeoLocation location)
    {
        if (!IsValidLongitude(location.Longitude) || !IsValidLatitude(location.Latitude))
        {
            throw new ArgumentException($"invalid longitude,latitude pair {location.Longitude},{location.Latitude}");
        }
        var geoSet = GeoLocations.GetOrAdd(key, _ => new SortedSet<GeoLocation>());
        if (geoSet.Any(loc => loc.Member == location.Member))
        {
            return 0; // Member already exists
        }
        geoSet.Add(location);
        return 1; // New member added
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
