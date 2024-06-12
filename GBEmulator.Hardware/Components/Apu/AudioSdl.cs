using System.Runtime.InteropServices;
using GBEmulator.Core.Interfaces;
using GBEmulator.Core.Options;
using Microsoft.Extensions.Logging;
using SDL2;


namespace GBEmulator.Hardware.Components.Apu;

public class AudioSdl : AudioDriver
{
    private bool _started { get; set; }
    private uint _deviceId { get; set; }
    private SDL.SDL_AudioCallback _callback;
    private TextWriter? _logger;
    private readonly AppSettings _appSettings;

    public AudioSdl(AppSettings appSettings, TextWriter logger) : base()
    {
        _started = false;
        _callback = Callback;
        _appSettings = appSettings;
        _logger = _appSettings.LoggingEnabled ? logger : null;
    }
    
    public override int Start(uint sampleRate, uint bufferSize)
    {
        _logger?.WriteLine("INF: Initializing audio subsystem");
        _logger?.Flush();
        
        SampleRate = sampleRate;
        BufferSize = bufferSize;
        
        // Initialise SDL audio
        if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) < 0) {
            _logger?.WriteLine($"ERR: Unable to initialize audio subsystem. {SDL.SDL_GetError()}");
            _logger?.Flush();
            return -1;
        }
        else {
            _logger?.WriteLine("INF: Initialized audio subsystem");
            _logger?.Flush();
        }

        var desiredSpec = new SDL.SDL_AudioSpec()
        {
            freq     = (int)SampleRate,
            format   = SDL.AUDIO_S16SYS,
            channels = 2,
            samples  = (ushort)BufferSize,
            callback = _callback,
            userdata = IntPtr.Zero,
        };


        SDL.SDL_AudioSpec obtained;

        _deviceId = SDL.SDL_OpenAudioDevice(null, 0, ref desiredSpec, out obtained, (int)SDL.SDL_AUDIO_ALLOW_FREQUENCY_CHANGE);

        if (_deviceId == 0) {
            _logger?.WriteLine($"ERR: Unable to open audio device. {SDL.SDL_GetError()}");
            _logger?.Flush();
            return -1;
        }
        else
        {
            _logger?.WriteLine("INF: Audio device opened successfully");
            _logger?.Flush();
        }

        Pause(false);
        _started = true;

        return 0;
    }

    public void Callback(IntPtr userdata, IntPtr stream, int len)
    {
        SDL.SDL_LockAudioDevice(_deviceId);
        
        // Convert the stream pointer to a managed array
        int length = len / sizeof(short);
        short[] audioStream = new short[length];
        Marshal.Copy(stream, audioStream, 0, length);

        int size = AudioBuffer.Count;

        if (size > length)
            size = length;
        else if (size < length)
            Array.Fill(audioStream, (short)0, size, length - size);

        AudioBuffer.CopyTo(0, audioStream, 0, size);
        AudioBuffer.RemoveRange(0, size);

        Marshal.Copy(audioStream, 0, stream, length);

        SDL.SDL_UnlockAudioDevice(_deviceId);
    }

    public override void Stop()
    {
        if (!_started)
            return;

        _started = false;

        SDL.SDL_PauseAudioDevice(_deviceId, 1);
        SDL.SDL_CloseAudioDevice(_deviceId);

        SDL.SDL_QuitSubSystem(SDL.SDL_INIT_AUDIO);

        _logger?.WriteLine("INF: Stopped Audio Device");
        _logger?.Flush();
    }

    public override void Pause(bool value)
    {
        SDL.SDL_PauseAudioDevice(_deviceId, value ? 1 : 0);
        _logger?.WriteLine(value ? "INF: Device paused" : "INF: Device un-paused");
        _logger?.Flush();
    }

    public override void SetSyncToAudio(bool syncToAudio)
    {
        SDL.SDL_LockAudioDevice(_deviceId);

        AudioBuffer = new List<short>();
        SyncToAudio = syncToAudio;

        SDL.SDL_UnlockAudioDevice(_deviceId);
    }
    
    public override void Reset()
    {
        SDL.SDL_LockAudioDevice(_deviceId);
        AudioBuffer = new List<short>();
        SDL.SDL_UnlockAudioDevice(_deviceId);
    }

    public override void InternalAddSample(short left, short right)
    {
        if (SyncToAudio) {
            while ((AudioBuffer.Count >> 1) > BufferSize)
                SDL.SDL_Delay(1);
        }

        AudioBuffer.Add(left);
        AudioBuffer.Add(right);
    }
}