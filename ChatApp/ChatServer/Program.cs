// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
using Server;
using Server.Net.IO;

class Program
{
    private static List<Client> _clients;
    static TcpListener _listener;
    static UdpClient _udpServer;
    
    static void Main(String[] args)
    {
        // Create list for clients / users on the server
        _clients = [];
        
        // Start listening for new clients
        Console.WriteLine("Listening...");
        _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
        _listener.Start();
        
        // UDP server
        _udpServer = new UdpClient(7891);
        

        // Keep accepting new clients
        Thread tcpThread = new Thread(ListenForTcpClients); 
        tcpThread.Start();

        // Keep listening for UDP packages
        Thread udpThread = new Thread(ListenForUdpPackets);
        udpThread.Start();
    }

    static void ListenForTcpClients()
    {
        Console.WriteLine("Listening for TCP clients...");
        while (true)
        {
            var client = new Client(_listener.AcceptTcpClient());
            _clients.Add(client);
            
            // Broadcast the connection to everyone on the server
            BroadcastConnection();
        }
    }

    static void ListenForUdpPackets()
    {
        IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
        Console.WriteLine("Listening for UDP packets...");
        while (true)
        {
            byte[] data = _udpServer.Receive(ref remoteEp);
            string message = Encoding.UTF8.GetString(data);
            Console.WriteLine(message);
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
                broadcastPacket.WriteString(cli.Username);
                broadcastPacket.WriteString(cli.Guid.ToString());
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