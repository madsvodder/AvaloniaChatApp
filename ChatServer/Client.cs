using System.Net.Sockets;
using System.Text;
using Server.Net.IO;

namespace Server;

public class Client
{
    public string Username { get; set; }
    public Guid Guid { get; set; }
    public TcpClient ClientSocket { get; set; }

    PacketReader _packetReader;
    public Client(TcpClient client)
    {
        ClientSocket = client;
        Guid = Guid.NewGuid();

        _packetReader = new PacketReader(ClientSocket.GetStream());

        var opcode = _packetReader.ReadByte();
        Username = _packetReader.ReadMessage();
        
        Console.WriteLine($"{DateTime.Now} - {Username} has connected to the server");
    }
}