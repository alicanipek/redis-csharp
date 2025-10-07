using System.Text;

namespace codecrafters_redis.Infrastructure;

public class RespParser
{
    public string EncodeRespArray(List<object> items)
    {
        var result = new System.Text.StringBuilder();
        result.Append($"*{items.Count}\r\n");
        foreach (var item in items)
        {
            if (item is string str)
            {
                result.Append(EncodeBulkString(str));
            }
            else if (item is int integer)
            {
                result.Append($":{integer}\r\n");
            }
            else if (item is Exception ex)
            {
                result.Append($"-{ex.Message}\r\n");
            }
            else if (item is List<object> list)
            {
                result.Append(EncodeRespArray(list));
            }
            else
            {
                throw new ArgumentException("Unsupported item type");
            }
        }
        return result.ToString();
    }
    public List<object> ParseRespArray(string resp)
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

    public string EncodeBulkString(string? str)
    {
        if (str == null)
        {
            return "$-1\r\n";
        }
        return $"${str.Length}\r\n{str}\r\n";
    }
}