using System;
using System.Buffers.Binary;
using System.Linq;
using System.Runtime.InteropServices;
using ChatApp.Net;
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
    
    private uint _seq;
    public ClientAudioHandler(Server server)
    {
        
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
            Format = SampleFormat.F32
        };
        
        _defaultAudioCaptureDevice = _engine.InitializeCaptureDevice(_defaultCaptureDeviceInfo, _defaultAudioFormat);
        
        //_defaultAudioCaptureDevice.OnAudioProcessed += DeviceOnOnAudioProcessed;
        
        //_defaultAudioCaptureDevice.Start();

        _recorder = new Recorder(_defaultAudioCaptureDevice, ProcessAudio);
        
        _defaultAudioCaptureDevice.Start();
        _recorder.StartRecording();
    }

    private void DeviceOnOnAudioProcessed(Span<float> samples, Capability capability)
    {
        Console.WriteLine(samples.Length);
        Console.WriteLine("Trying to sample audio");
    }
    
    private void ProcessAudio(Span<float> samples, Capability capability)
    {
        // If you instead want 16-bit PCM on the wire, quantize here.
        // For now: send float32 PCM bytes as-is.
        ReadOnlySpan<byte> pcm = MemoryMarshal.AsBytes(samples);

        
    }
}