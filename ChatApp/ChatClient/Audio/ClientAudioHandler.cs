using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using ChatApp.Net;
using Concentus.Enums;
using Concentus.Structs;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Structs;

namespace ChatApp.Audio;

public class ClientAudioHandler
{

    private MiniAudioEngine _engine;

    private DeviceInfo _defaultCaptureDeviceInfo;
    
    private AudioFormat _defaultAudioFormat;
    
    private AudioCaptureDevice _defaultAudioCaptureDevice;
    
    private Recorder _recorder;
    
    private Server _server;
    
    private OpusEncoder _encoder;
    
    private uint _seq;
    public ClientAudioHandler(Server server)
    {
        
        _encoder = new OpusEncoder(48000, 1, OpusApplication.OPUS_APPLICATION_VOIP);
        _encoder.Bitrate = 64000;
        
        // Server
        _server = server;
        
        // Initialize the engine
        _engine = new MiniAudioEngine();
        
        // Set the default device
        _defaultCaptureDeviceInfo = _engine.CaptureDevices.FirstOrDefault(d => d.IsDefault);
        if (_defaultCaptureDeviceInfo.Id == IntPtr.Zero)
        {
            Console.WriteLine("Default capture device not found");
            return;
        }
        
        // Define the audio format
        _defaultAudioFormat = new AudioFormat
        {
            SampleRate = 48000,
            Channels = 1,
            Format = SampleFormat.S16
        };
        
        _defaultAudioCaptureDevice = _engine.InitializeCaptureDevice(_defaultCaptureDeviceInfo, _defaultAudioFormat);
        
        _defaultAudioCaptureDevice.OnAudioProcessed += DefaultAudioCaptureDeviceOnOnAudioProcessed;
        
        _defaultAudioCaptureDevice.Start();
    }

    private void DefaultAudioCaptureDeviceOnOnAudioProcessed(Span<float> samples, Capability capability)
    {
        Span<short> pcm = stackalloc short[samples.Length];
        
        // Convert float samples (-1.0 to 1.0) to 16-bit PCM (short)
        for (int i = 0; i < samples.Length; i++)
            pcm[i] = (short)(samples[i] * short.MaxValue);

        // Convert short[] to byte[] for network transmission
        byte[] buffer = MemoryMarshal.AsBytes(pcm).ToArray();

        if (_server.Status == Server.ConnectionStatus.Connected)
            _server.sendAudioPacketsToServer(buffer, buffer.Length);
    }
}