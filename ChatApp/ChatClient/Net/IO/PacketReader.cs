using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ChatApp.Net.IO;

public class PacketReader : BinaryReader
{
    
    private NetworkStream _ns;
    public PacketReader(NetworkStream ns) : base(ns)
    {
        _ns = ns;
    }
    public string ReadMessage()
    {
        // Reads the length that is written in the stream, and the message
        int length = ReadInt32();
        byte[] buffer = ReadBytes(length);
        return Encoding.UTF8.GetString(buffer);
    }
}