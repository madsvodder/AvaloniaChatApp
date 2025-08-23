using Concentus.Enums;
using Concentus.Structs;

namespace Server;

public class Audio
{
    
    OpusEncoder _encoder;
    
    public Audio()
    {
        _encoder = new OpusEncoder(4800, 1, OpusApplication.OPUS_APPLICATION_VOIP);
    }
    
    
}