using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();
using Socket socket = server.AcceptSocket(); // wait for client
socket.Send(System.Text.Encoding.ASCII.GetBytes("+PONG\r\n"));