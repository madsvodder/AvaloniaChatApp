using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Data;
using ChatApp.Net.IO;

namespace ChatApp.Net;

// This class is not the actual server. This is all the server stuff, that happens for the client
// It is used to connect, and send messages to the server itself
// It also constantly reads packets from the server, which is usually client messages and broadcasts
public class Server
{
    
    // Enum for connection status
    public enum ConnectionStatus 
    {
        Disconnected,
        Connecting,
        Connected
    }
    
    // Properties
    public ConnectionStatus Status = ConnectionStatus.Disconnected;
    
    // Events
    public event Action ConnectedEvent;
    public event Action DisconnectedEvent;
    public event Action MsgReceivedEvent;
    
    // Vars
    private TcpClient _client;
    private UdpClient _udpClient;
    private PacketBuilder _packetBuilder;
    public PacketReader PacketReader;
    public Server()
    {
        _client = new TcpClient();
        _udpClient = new UdpClient();
    }

    public event Action<ConnectionStatus> StatusChanged;
    
    private void SetStatus(ConnectionStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(status);
    }

    public void ConnectToServer(string ip, int port, string username)
    {
        
        // If already connected, do nothing
        if (_client.Connected) return;
        
        try
        {
            // Connect to server TCP
            _client.Connect(ip, port);
            PacketReader = new PacketReader(_client.GetStream());
            
            // UDP Test
            string udpMessage = $"UDP Test from {username}";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(udpMessage);
            _udpClient.Send(data, data.Length, ip, port);
            
            // Send packet to server
            var connectPacket = new PacketBuilder();
            connectPacket.WriteOpCode(0);
            connectPacket.WriteString(username);
            _client.Client.Send(connectPacket.GetPacketBytes());
            
            // read packets
            ReadPacketsLoop();
            
            // Debug
            Console.WriteLine("Connected to server successfully.");
            
            // Set connection status
            SetStatus(ConnectionStatus.Connected);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to connect to server: " + ex.Message);
            SetStatus(ConnectionStatus.Disconnected);
        }
    }

    // Send a message to the server using PacketBuilder (opcode - 5)
    public void SendMessageToServer(string message)
    {
        var messagePacket = new PacketBuilder();
        messagePacket.WriteOpCode(5);
        messagePacket.WriteString(message);
        _client.Client.Send(messagePacket.GetPacketBytes());
    }

    public void sendAudioPacketsToServer(byte[] buffer, int bufferLength)
    {
        IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7891); // Replace with your server's IP and port
        _udpClient.Send(buffer, bufferLength, _serverEndPoint);
        Console.WriteLine("Sent audio packets to server.");
    }

    // Loop for reading packets - Depending on the opcode, do different stuff
    private void ReadPacketsLoop()
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