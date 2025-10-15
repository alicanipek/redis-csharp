using System;

namespace codecrafters_redis.src.Models;

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