using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();

// Buffer for reading data
Byte[] bytes = new Byte[256];

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
            byte[] msg = System.Text.Encoding.ASCII.GetBytes("+PONG\r\n");

            // Send back a response.
            stream.Write(msg, 0, msg.Length);
        }
    });
    thread.Start();
}