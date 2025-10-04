using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();

// Buffer for reading data
Byte[] bytes = new Byte[256];

Dictionary<string, string> store = new Dictionary<string, string>();

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
                store[parsed[1].ToString()] = parsed[2].ToString();
                msg = System.Text.Encoding.ASCII.GetBytes("+OK\r\n");
            }
            else if (parsed[0].ToString() == "GET" && parsed.Count > 1)
            {
                if (store.TryGetValue(parsed[1].ToString(), out var value))
                {
                    msg = System.Text.Encoding.ASCII.GetBytes(EncodeBulkString(value));
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