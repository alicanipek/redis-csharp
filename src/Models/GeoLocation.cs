using System;

namespace codecrafters_redis.src.Models;

public class GeoLocation
{
    private const double MIN_LATITUDE = -85.05112878;
    private const double MAX_LATITUDE = 85.05112878;
    private const double MIN_LONGITUDE = -180;
    private const double MAX_LONGITUDE = 180;
    private const double EARTH_RADIUS_IN_METERS = 6372797.560856;
    private const double LATITUDE_RANGE = MAX_LATITUDE - MIN_LATITUDE;
    private const double LONGITUDE_RANGE = MAX_LONGITUDE - MIN_LONGITUDE;
    private const double D_R = Math.PI / 180; // Degrees to radians
    private static double deg_rad(double ang) { return ang * D_R; }
    private static double rad_deg(double ang) { return ang / D_R; }

    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string Member { get; set; }

    public GeoLocation(double longitude, double latitude, string member)
    {
        Longitude = longitude;
        Latitude = latitude;
        Member = member;
    }

    public GeoLocation(double geoCode, string member)
    {
        FromGeocode((long)geoCode);
        Member = member;
    }

    public long Encode()
    {
        // Normalize to the range 0-2^26
        double normalizedLatitude = Math.Pow(2, 26) * (Latitude - MIN_LATITUDE) / LATITUDE_RANGE;
        double normalizedLongitude = Math.Pow(2, 26) * (Longitude - MIN_LONGITUDE) / LONGITUDE_RANGE;

        // Truncate to integers
        int normalizedLatitudeInt = (int)normalizedLatitude;
        int normalizedLongitudeInt = (int)normalizedLongitude;

        return Interleave(normalizedLatitudeInt, normalizedLongitudeInt);
    }

    private static long Interleave(int x, int y)
    {
        long spreadX = SpreadInt32ToInt64(x);
        long spreadY = SpreadInt32ToInt64(y);
        long yShifted = spreadY << 1;
        return spreadX | yShifted;
    }

    private static long SpreadInt32ToInt64(int v)
    {
        long result = v & 0xFFFFFFFF;
        result = (result | (result << 16)) & 0x0000FFFF0000FFFF;
        result = (result | (result << 8)) & 0x00FF00FF00FF00FF;
        result = (result | (result << 4)) & 0x0F0F0F0F0F0F0F0F;
        result = (result | (result << 2)) & 0x3333333333333333;
        result = (result | (result << 1)) & 0x5555555555555555;
        return result;
    }

    private void FromGeocode(long geoCode)
    {
        // Align bits of both latitude and longitude to take even-numbered position
        long y = geoCode >> 1;
        long x = geoCode;

        // Compact bits back to 32-bit ints
        int gridLatitudeNumber = CompactInt64ToInt32(x);
        int gridLongitudeNumber = CompactInt64ToInt32(y);

        (Latitude, Longitude) = ConvertGridNumbersToCoordinates(gridLatitudeNumber, gridLongitudeNumber);
    }

    /// <summary>
    /// Compact a 64-bit integer with interleaved bits back to a 32-bit integer.
    /// This is the reverse operation of spread_int32_to_int64.
    /// </summary>
    /// <param name="v">The 64-bit integer with interleaved bits</param>
    /// <returns>Compacted 32-bit integer</returns>
    private static int CompactInt64ToInt32(long v)
    {
        v = v & 0x5555555555555555;
        v = (v | (v >> 1)) & 0x3333333333333333;
        v = (v | (v >> 2)) & 0x0F0F0F0F0F0F0F0F;
        v = (v | (v >> 4)) & 0x00FF00FF00FF00FF;
        v = (v | (v >> 8)) & 0x0000FFFF0000FFFF;
        v = (v | (v >> 16)) & 0x00000000FFFFFFFF;
        return (int)v;
    }

    /// <summary>
    /// Convert grid numbers back to geographic coordinates
    /// </summary>
    /// <param name="gridLatitudeNumber">Grid latitude number</param>
    /// <param name="gridLongitudeNumber">Grid longitude number</param>
    /// <returns>Tuple containing (latitude, longitude)</returns>
    private static (double latitude, double longitude) ConvertGridNumbersToCoordinates(int gridLatitudeNumber, int gridLongitudeNumber)
    {
        // Calculate the grid boundaries
        double gridLatitudeMin = MIN_LATITUDE + LATITUDE_RANGE * (gridLatitudeNumber / Math.Pow(2, 26));
        double gridLatitudeMax = MIN_LATITUDE + LATITUDE_RANGE * ((gridLatitudeNumber + 1) / Math.Pow(2, 26));
        double gridLongitudeMin = MIN_LONGITUDE + LONGITUDE_RANGE * (gridLongitudeNumber / Math.Pow(2, 26));
        double gridLongitudeMax = MIN_LONGITUDE + LONGITUDE_RANGE * ((gridLongitudeNumber + 1) / Math.Pow(2, 26));

        // Calculate the center point of the grid cell
        double latitude = (gridLatitudeMin + gridLatitudeMax) / 2;
        double longitude = (gridLongitudeMin + gridLongitudeMax) / 2;

        return (latitude, longitude);
    }

    public double GeohashGetDistance(GeoLocation other)
    {
        double lat1r, lon1r, lat2r, lon2r, u, v, a;
        lon1r = deg_rad(Longitude);
        lon2r = deg_rad(other.Longitude);
        v = Math.Sin((lon2r - lon1r) / 2);
        /* if v == 0 we can avoid doing expensive math when lons are practically the same */
        if (v == 0.0)
            return GeohashGetLatDistance(Latitude, other.Latitude);
        lat1r = deg_rad(Latitude);
        lat2r = deg_rad(other.Latitude);
        u = Math.Sin((lat2r - lat1r) / 2);
        a = u * u + Math.Cos(lat1r) * Math.Cos(lat2r) * v * v;
        return 2.0 * EARTH_RADIUS_IN_METERS * Math.Asin(Math.Sqrt(a));
    }
    private static double GeohashGetLatDistance(double lat1d, double lat2d)
    {
        return EARTH_RADIUS_IN_METERS * Math.Abs(deg_rad(lat2d) - deg_rad(lat1d));
    }

    public int GeohashGetDistanceIfInRadius(GeoLocation other, double radius)
    {
        var d = GeohashGetDistance(other);
        if (d > radius) return 0;
        return 1;
    }
}