using System;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.Infrastructure;

public class LengthEncoding
{
    public int Length { get; set; }
    public bool IsNumber { get; set; }
}

public class RDBFile
{
    public string MagicString { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    public Dictionary<int, Dictionary<string, Item>> Databases { get; } = new();
}

public static class RDBFileParser
{
    private static RDBFile dataModel = new() { Metadata = new Dictionary<string, string>() };
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static async Task<RDBFile?> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[1024];
        int count = await fileStream.ReadAsync(buffer, 0, 9);
        if (count != 9)
        {
            throw new InvalidDataException("Unexpected end of file. Expected 9 bytes in the header.");
        }
        dataModel.MagicString = System.Text.Encoding.ASCII.GetString(buffer);


        while (true)
        {
            var auxByte = (byte)fileStream.ReadByte();
            if (auxByte != 0xFA)
            {
                fileStream.Position -= 1;
                break;
            }
            var key = await ReadString(fileStream);
            var value = await ReadString(fileStream);
            dataModel.Metadata[key] = value;
        }
        while (true)
        {
            var op = (byte)fileStream.ReadByte();
            if (op != 0xFE)
            {
                fileStream.Position -= 1;
                break;
            }
            var dbNumber = await ParseLengthEncoding(fileStream);
            var db = await ParseDatabaseAsync(fileStream);
            dataModel.Databases.Add(dbNumber.Length, db);
        }
        return dataModel;
    }

    private static async Task<Dictionary<string, Item>> ParseDatabaseAsync(FileStream fileStream)
    {
        var db = new Dictionary<string, Item>();
        var hashTableMarker = (byte)fileStream.ReadByte();
        if (hashTableMarker != 0xFB)
        {
            throw new InvalidDataException("Expected hash table marker.");
        }

        var size = await ParseLengthEncoding(fileStream);
        var expirySize = await ParseLengthEncoding(fileStream);

        for (int i = 0; i < size.Length; i++)
        {
            DateTime? expiry = null;
            var valueType = (byte)fileStream.ReadByte();
            if (valueType == 0xFC)
            {
                var buffer = new byte[8];
                await fileStream.ReadExactlyAsync(buffer, 0, 8);
                var longTimeout = BitConverter.ToUInt64(buffer, 0);
                expiry = Epoch.AddMilliseconds(longTimeout);
                valueType = (byte)fileStream.ReadByte();
            }
            else if (valueType == 0xFD)
            {
                var buffer = new byte[4];
                await fileStream.ReadExactlyAsync(buffer, 0, 4);
                var intTimeout = BitConverter.ToInt32(buffer, 0);
                expiry = Epoch.AddSeconds(intTimeout);
                valueType = (byte)fileStream.ReadByte();
            }

            if (valueType != 0x00)
            {
                throw new NotSupportedException($"Value type {valueType} not supported.");
            }

            var key = await ReadString(fileStream);
            var value = await ReadString(fileStream);
            db.Add(key, new Item { Value = value, Expiration = expiry });
        }
        return db;

    }

    private static async Task<LengthEncoding> ParseLengthEncoding(System.IO.Stream stream)
    {
        var firstByte = (byte)stream.ReadByte();
        if ((firstByte & 0xC0) == 0x00)
        {
            return new LengthEncoding { Length = (firstByte & 0x3F), IsNumber = false };
        }
        else if ((firstByte & 0xC0) == 0x40)
        {
            var secondByte = (byte)stream.ReadByte();
            return new LengthEncoding { Length = (((firstByte & 0x3F) << 8) | secondByte), IsNumber = false };
        }
        else if ((firstByte & 0xC0) == 0x80)
        {
            var buffer = new byte[4];
            await stream.ReadExactlyAsync(buffer, 0, 4);
            return new LengthEncoding { Length = BitConverter.ToInt32(buffer, 0), IsNumber = false };
        }else if ((firstByte & 0xC0) == 0xC0)
        {
            var length = firstByte & 0x3F;
            if (length == 0)
            {
                return new LengthEncoding { Length = stream.ReadByte(), IsNumber = true };
            }
            else if (length == 1)
            {
                var buffer = new byte[2];
                await stream.ReadExactlyAsync(buffer, 0, 2);
                return new LengthEncoding { Length = BitConverter.ToInt16(buffer, 0), IsNumber = true };
            }
            else if (length == 2)
            {
                var buffer = new byte[4];
                await stream.ReadExactlyAsync(buffer, 0, 4);
                return new LengthEncoding { Length = BitConverter.ToInt32(buffer, 0), IsNumber = true };
            }else
            {
                throw new NotSupportedException("Length encoding type not supported.");
            }
        }
        else
        {
            throw new NotSupportedException("Length encoding type not supported.");
        }
    }

    private static async Task<string> ReadString(FileStream fileStream)
    {
        var lengthEncoding = await ParseLengthEncoding(fileStream);
        if (lengthEncoding.IsNumber)
        {
            return lengthEncoding.Length.ToString();
        }
        var stringBuffer = new byte[lengthEncoding.Length];
        int bytesRead = await fileStream.ReadAsync(stringBuffer, 0, lengthEncoding.Length);
        if (bytesRead < lengthEncoding.Length)
        {
            throw new EndOfStreamException("Unexpected end of stream while reading string data.");
        }

        return System.Text.Encoding.ASCII.GetString(stringBuffer);
    }
}
