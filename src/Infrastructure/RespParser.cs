using System.Text;

namespace codecrafters_redis.src.Infrastructure;

public static class RespParser
{
    public const string OkString = "+OK\r\n";
    public const string NullBulkString = "$-1\r\n";
    public static readonly byte[] OkBytes = Encoding.UTF8.GetBytes(OkString);
    public static readonly byte[] NullBulkBytes = Encoding.UTF8.GetBytes(NullBulkString);
    public static readonly byte[] NullBulkStringArrayBytes = Encoding.UTF8.GetBytes("*-1\r\n");
    public static readonly byte[] EmptyBulkStringArrayBytes = Encoding.UTF8.GetBytes("*0\r\n");

    public static List<object> ParseRespArray(string resp)
    {
        var result = new List<object>();
        var lines = resp.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        int i = 0;
        while (i < lines.Length)
        {
            if (lines[i].StartsWith("*"))
            {
                int arrayLength = int.Parse(lines[i].Substring(1));
                i++;
                for (int j = 0; j < arrayLength; j++)
                {
                    if (lines[i].StartsWith("$"))
                    {
                        int strLength = int.Parse(lines[i].Substring(1));
                        i++;
                        result.Add(lines[i]);
                        i++;
                    }
                    else if (lines[i].StartsWith(":"))
                    {
                        result.Add(int.Parse(lines[i].Substring(1)));
                        i++;
                    }
                    else if (lines[i].StartsWith("+"))
                    {
                        result.Add(lines[i].Substring(1));
                        i++;
                    }
                    else if (lines[i].StartsWith("-"))
                    {
                        result.Add(new Exception(lines[i].Substring(1)));
                        i++;
                    }
                }
            }
            else
            {
                i++;
            }
        }
        return result;
    }

    public static byte[] EncodeBulkStringArrayBytes(string[] strings)
    {
        if (strings.Length == 0) return EmptyBulkStringArrayBytes;
        
        var sb = new StringBuilder();
        sb.Append('*');
        sb.Append(strings.Length);
        sb.Append("\r\n");
        foreach (var s in strings)
        {
            sb.Append(EncodeBulkString(s));
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public static byte[] EncodeSimpleString(string s)
    {
        return Encoding.UTF8.GetBytes($"+{s}\r\n");
    }

    public static byte[] EncodeBulkStringBytes(string? str)
    {
        var s = "";
        if (str == null)
        {
            s = NullBulkString;
        }
        else
        {
            s = $"${str.Length}\r\n{str}\r\n";
        }
        return Encoding.UTF8.GetBytes(s);
    }

    public static byte[] EncodeBulkStringBytes(List<string> list)
    {
        var s = "";
        if (list == null || list.Count == 0)
        {
            s = NullBulkString;
        }
        else
        {
            s = EncodeBulkString(list);
        }
        return Encoding.UTF8.GetBytes(s);
    }

    public static string EncodeBulkString(string? str)
    {
        if (str == null)
        {
            return NullBulkString;
        }
        return $"${str.Length}\r\n{str}\r\n";
    }

    public static string EncodeBulkString(List<string> list)
    {
        var sb = new StringBuilder();
        var length = list.Select(s => s.Length).Sum();
        sb.Append($"${length}\r\n");
        foreach (var str in list)
        {
            sb.Append($"{str}");
        }
        sb.Append("\r\n");
        return sb.ToString();
    }

    public static byte[] EncodeErrorString(string message)
    {
        return Encoding.UTF8.GetBytes($"-ERR {message}\r\n");
    }

    public static byte[] EncodeInteger(long number)
    {
        return Encoding.UTF8.GetBytes($":{number}\r\n");
    }

}