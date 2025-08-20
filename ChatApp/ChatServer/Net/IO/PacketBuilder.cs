using System.Text;

namespace Server.Net.IO;

public class PacketBuilder
{
    MemoryStream _stream;
    public PacketBuilder()
    {
        _stream = new MemoryStream();
    }

    public void WriteOpCode(byte opcode)
    {
        _stream.WriteByte(opcode);
    }

    public void WriteString(string str)
    {
        // Get the length of the String and write it to the stream
        var bytes = Encoding.UTF8.GetBytes(str);
        
        // Write the length to the stream as an integer
        _stream.Write(BitConverter.GetBytes(bytes.Length));
        
        // Write the message / string itself
        _stream.Write(Encoding.UTF8.GetBytes(str));
    }

    public byte[] GetPacketBytes()
    {
        return _stream.ToArray();
    }
}