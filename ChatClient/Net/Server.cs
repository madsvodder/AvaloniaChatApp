using System;
using System.Net.Sockets;
using ChatApp.Net.IO;

namespace ChatApp.Net;

public class Server
{
    
    // Vars
    TcpClient _client;
    PacketBuilder _packetBuilder;
    private PacketReader _packetReader;
    public Server()
    {
        _client = new TcpClient();
    }

    public void ConnectToServer(string ip, int port, string username)
    {
        
        // If already connected, do nothing
        if (_client.Connected) return;
        
        try
        {
            // Connect to server
            _client.Connect(ip, port);
            
            // Send packet to server
            var connectPacket = new PacketBuilder();
            connectPacket.WriteOpCode(0);
            connectPacket.WriteString(username);
            _client.Client.Send(connectPacket.GetPacketBytes());
            
            // Setup packet reader to read messages
            _packetReader = new PacketReader(_client.GetStream());
            
            // Debug
            Console.WriteLine("Connected to server successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to connect: " + ex.Message);
        }
    }
}