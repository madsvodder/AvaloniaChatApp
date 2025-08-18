using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Data;
using ChatApp.Net.IO;

namespace ChatApp.Net;

public class Server
{
    
    // Events
    public event Action ConnectedEvent;
    public event Action DisconnectedEvent;
    public event Action MsgReceivedEvent;
    
    // Vars
    private TcpClient _client;
    private PacketBuilder _packetBuilder;
    public PacketReader PacketReader;
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
            PacketReader = new PacketReader(_client.GetStream());
            
            // Send packet to server
            var connectPacket = new PacketBuilder();
            connectPacket.WriteOpCode(0);
            connectPacket.WriteString(username);
            _client.Client.Send(connectPacket.GetPacketBytes());
            
            // read packets
            ReadPackets();
            
            // Debug
            Console.WriteLine("Connected to server successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to connect: " + ex.Message);
        }
    }

    public void SendMessageToServer(string message)
    {
        var messagePacket = new PacketBuilder();
        messagePacket.WriteOpCode(5);
        messagePacket.WriteString(message);
        _client.Client.Send(messagePacket.GetPacketBytes());
    }

    private void ReadPackets()
    {
        Console.WriteLine("Reading packets...");
        Task.Run((() =>
        {
            while (true)
            {
                var opcode = PacketReader.ReadByte();
                switch (opcode)
                {
                    case 1:
                        // Connected event
                        ConnectedEvent?.Invoke();
                    break;
                    
                    case 5:
                        // Message event
                        MsgReceivedEvent?.Invoke();
                        break;
                    
                    case 10:
                        // Disconnect event
                        DisconnectedEvent?.Invoke();
                        break;
                    
                    default:
                        Console.WriteLine($"Unknown opcode {opcode}");
                        break;
                }
            }
        }));
    }
}