using System.Text;

namespace codecrafters_redis.src.Infrastructure;

public static class RespParser
{
    public const string OkString = "+OK\r\n";
    public const string NullBulkString = "$-1\r\n";
    public static readonly string EmptyArray = "*0\r\n";
    public static readonly string NullArray = "*-1\r\n";
    public static readonly byte[] OkBytes = Encoding.UTF8.GetBytes(OkString);
    public static readonly byte[] NullBulkStringBytes = Encoding.UTF8.GetBytes(NullBulkString);
    public static readonly byte[] NullArrayBytes = Encoding.UTF8.GetBytes(NullArray);
    public static readonly byte[] EmptyArrayBytes = Encoding.UTF8.GetBytes(EmptyArray);

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

    public static string EncodeRespArray(object[] items)
    {
        if (items.Length == 0) return EmptyArray;

        var sb = new StringBuilder();
        sb.Append('*');
        sb.Append(items.Length);
        sb.Append("\r\n");
        foreach (var item in items)
        {
            switch (item)
            {
                case null:
                    sb.Append(NullArray);
                    break;
                case string s:
                    sb.Append(EncodeBulkString(s));
                    break;
                case int n:
                    sb.Append(EncodeInteger(n));
                    break;
                case long n:
                    sb.Append(EncodeInteger(n));
                    break;
                case string[] arr:
                    sb.Append(EncodeRespArray(arr));
                    break;
                case Exception e:
                    sb.Append($"-ERR {e.Message}\r\n");
                    break;
                default:
                    throw new NotSupportedException($"Type {item.GetType()} not supported.");
            }
        }
        return sb.ToString();
    }

    public static byte[] EncodeRespArrayBytes(object[] items)
    {
        return Encoding.UTF8.GetBytes(EncodeRespArray(items));
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

    public static byte[] EncodeBulkStringBytes(string[] list)
    {
        var s = "";
        if (list == null || list.Length == 0)
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

    public static string EncodeBulkString(string[] list)
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

    public static byte[] EncodeIntegerBytes(long number)
    {
        return Encoding.UTF8.GetBytes($":{number}\r\n");
    }

    public static string EncodeInteger(int number)
    {
        return $":{number}\r\n";
    }

    public static string EncodeInteger(long number)
    {
        return $":{number}\r\n";
    }

}