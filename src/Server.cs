using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();

// Buffer for reading data
Byte[] bytes = new Byte[256];

Dictionary<string, Item> store = new Dictionary<string, Item>();

// Enter the listening loop.
while (true)
{
    Console.Write("Waiting for a connection... ");

    // Perform a blocking call to accept requests.
    // You could also use server.AcceptSocket() here.
    TcpClient client = server.AcceptTcpClient();


    var thread = new Thread(() =>
    {
        // Get a stream object for reading and writing
        NetworkStream stream = client.GetStream();

        int i;

        // Loop to receive all the data sent by the client.
        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
        {
            string request = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
            List<object> parsed = ParseRespArray(request);
            foreach (var item in parsed)
            {
                Console.Write(item + " ");
            }
            byte[] msg = [];
            if (parsed[0].ToString() == "PING")
            {
                msg = System.Text.Encoding.ASCII.GetBytes("+PONG\r\n");
            }
            else if (parsed[0].ToString() == "ECHO" && parsed.Count > 1)
            {
                msg = System.Text.Encoding.ASCII.GetBytes(EncodeBulkString(parsed[1].ToString()));
            }
            else if (parsed[0].ToString() == "SET" && parsed.Count > 2)
            {
                var key = parsed[1].ToString();
                var value = parsed[2].ToString();
                if (parsed.Count > 3 && parsed[3].ToString().ToUpper() == "PX" && parsed.Count > 4)
                {
                    var px = int.Parse(parsed[4].ToString());
                    store[key] = new Item { Value = value, Expiration = DateTime.Now.AddMilliseconds(px) };
                }
                else
                {
                    store[key] = new Item { Value = value, Expiration = null };
                }
                msg = System.Text.Encoding.ASCII.GetBytes("+OK\r\n");
            }
            else if (parsed[0].ToString() == "GET" && parsed.Count > 1)
            {
                if (store.TryGetValue(parsed[1].ToString(), out var item))
                {
                    string value = null;
                    if (item.Expiration == null || item.Expiration > DateTime.Now)
                    {
                        value = item.Value.ToString();
                    }
                    else
                    {
                        store.Remove(parsed[1].ToString());
                    }
                    // Return the value
                    msg = System.Text.Encoding.ASCII.GetBytes(EncodeBulkString(value));
                }
                else
                {
                    msg = System.Text.Encoding.ASCII.GetBytes("$-1\r\n");
                }
            }
            else if (parsed[0].ToString() == "RPUSH" && parsed.Count > 2)
            {
                var key = parsed[1].ToString();
                var values = parsed.Skip(2).Select(v => v.ToString()).ToList();

                if (!store.ContainsKey(key))
                {
                    store[key] = new Item { Value = new List<string>(), Expiration = null };
                }
                foreach (var value in values)
                {
                    ((List<string>)store[key].Value).Add(value);
                }
                msg = System.Text.Encoding.ASCII.GetBytes(":" + ((List<string>)store[key].Value).Count + "\r\n");
            }
            else if (parsed[0].ToString() == "LRANGE" && parsed.Count > 3)
            {
                var key = parsed[1].ToString();
                var start = int.Parse(parsed[2].ToString());
                var stop = int.Parse(parsed[3].ToString());

                if (store.TryGetValue(key, out var item) && item.Value is List<string> list)
                {
                    if (item.Expiration != null && item.Expiration <= DateTime.Now)
                    {
                        store.Remove(key);
                        msg = System.Text.Encoding.ASCII.GetBytes("*0\r\n");
                    }
                    else
                    {
                        // Handle negative indices
                        if (start < 0) start = list.Count + start < 0 ? 0 : list.Count + start;
                        if (stop < 0) stop = list.Count + stop < 0 ? 0 : list.Count + stop;

                        // Adjust stop to be inclusive
                        stop = Math.Min(stop, list.Count - 1);

                        if (start > stop || start >= list.Count)
                        {
                            msg = System.Text.Encoding.ASCII.GetBytes("*0\r\n");
                        }
                        else
                        {
                            var range = list.GetRange(start, stop - start + 1);
                            var response = $"*{range.Count}\r\n";
                            foreach (var val in range)
                            {
                                response += EncodeBulkString(val);
                            }
                            msg = System.Text.Encoding.ASCII.GetBytes(response);
                        }
                    }
                }
                else
                {
                    msg = System.Text.Encoding.ASCII.GetBytes("*0\r\n");
                }
            }
            else if (parsed[0].ToString() == "LPUSH" && parsed.Count > 2)
            {
                var key = parsed[1].ToString();
                var values = parsed.Skip(2).Select(v => v.ToString()).ToList();

                if (!store.ContainsKey(key))
                {
                    store[key] = new Item { Value = new List<string>(), Expiration = null };
                }
                foreach (var value in values)
                {
                    ((List<string>)store[key].Value).Insert(0, value);
                }
                msg = System.Text.Encoding.ASCII.GetBytes(":" + ((List<string>)store[key].Value).Count + "\r\n");
            }
            else if (parsed[0].ToString() == "LLEN" && parsed.Count > 1)
            {
                var key = parsed[1].ToString();
                if (store.TryGetValue(key, out var item) && item.Value is List<string> list)
                {
                    if (item.Expiration != null && item.Expiration <= DateTime.Now)
                    {
                        store.Remove(key);
                        msg = System.Text.Encoding.ASCII.GetBytes(":0\r\n");
                    }
                    else
                    {
                        msg = System.Text.Encoding.ASCII.GetBytes(":" + list.Count + "\r\n");
                    }
                }
                else
                {
                    msg = System.Text.Encoding.ASCII.GetBytes(":0\r\n");
                }
            }
            else if (parsed[0].ToString() == "LPOP" && parsed.Count > 1)
            {
                var key = parsed[1].ToString();

                if (store.TryGetValue(key, out var item) && item.Value is List<string> list)
                {
                    var popped = ((List<string>)store[key].Value)[0];
                    ((List<string>)store[key].Value).RemoveAt(0);
                    
                    msg = System.Text.Encoding.ASCII.GetBytes(EncodeBulkString(popped));
                }
                else
                {
                    msg = System.Text.Encoding.ASCII.GetBytes("$-1\r\n");
                }

            }
            else
            {
                msg = System.Text.Encoding.ASCII.GetBytes("-ERR unknown command\r\n");
            }

            stream.Write(msg, 0, msg.Length);
            // Send back a response.
        }
    });
    thread.Start();
}

string EncodeBulkString(string str)
{
    if (str == null)
    {
        return "$-1\r\n";
    }
    return $"${str.Length}\r\n{str}\r\n";
}

List<object> ParseRespArray(string resp)
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