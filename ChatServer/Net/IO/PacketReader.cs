using System.Net.Sockets;
using System.Text;

namespace Server.Net.IO;

public class PacketReader : BinaryReader
{
    
    private NetworkStream _ns;
    public PacketReader(NetworkStream ns) : base(ns)
    {
        _ns = ns;
    }
    public string readMessage()
    {
        byte[] msgBuffer;
        var length = ReadInt32();
        msgBuffer = new byte[length];
        _ns.Read(msgBuffer, 0, length);
        
        var msg = Encoding.ASCII.GetString(msgBuffer);
        return msg;
    }
}