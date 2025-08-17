using System;
using System.IO;
using System.Text;

namespace ChatApp.Net.IO;

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
        var strLength = str.Length;
        _stream.Write(BitConverter.GetBytes(strLength));
        _stream.Write(Encoding.ASCII.GetBytes(str));
    }

    public byte[] GetPacketBytes()
    {
        return _stream.ToArray();
    }
}