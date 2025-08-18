// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using Server;
using Server.Net.IO;

class Program
{
    private static List<Client> _clients;
    static TcpListener _listener;
    
    static void Main(String[] args)
    {
        // Create list for clients / users on the server
        _clients = new List<Client>();
        
        // Start listening for new clients
        Console.WriteLine("Listening...");
        _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
        _listener.Start();

        // Keep accepting new clients
        while (true)
        {
            var client = new Client(_listener.AcceptTcpClient());
            _clients.Add(client);
            
            // Broadcast the connection to everyone on the server
            BroadcastConnection();
        }
    }

    static void BroadcastConnection()
    {
        foreach (var client in _clients)
        {
            foreach (var cli in _clients)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(1);
                broadcastPacket.WriteString($"{cli.Username} connected to the server");
                client.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }
        }
    }
}