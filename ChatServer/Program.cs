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
        _clients = [];
        
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

    public static void BroadcastMessage(string message)
    {
        foreach (var client in _clients)
        {
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(5);
            msgPacket.WriteString(message);
            client.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
        }
    }
    
    public static void BroadcastDisconnect(string guid)
    {
        var disconnectedClient = _clients.Where(x => x.Guid.ToString() == guid).FirstOrDefault();
        _clients.Remove(disconnectedClient);
        foreach (var client in _clients)
        {
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(10);
            msgPacket.WriteString(guid);
            client.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
        }
        
        BroadcastMessage($"{disconnectedClient.Username} disconnected from the server");
    }
}