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
        if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) < 0)
        {
            _logger?.WriteLine($"ERR: Unable to initialize audio subsystem. {SDL.SDL_GetError()}");
            _logger?.Flush();
            return -1;
        }
        else
        {
            _logger?.WriteLine("INF: Initialized audio subsystem");
            _logger?.Flush();
        }

        var desiredSpec = new SDL.SDL_AudioSpec()
        {
            freq = (int)SampleRate,
            format = SDL.AUDIO_S16SYS,
            channels = 2,
            samples = (ushort)BufferSize,
            callback = _callback,
            userdata = IntPtr.Zero,
        };


        SDL.SDL_AudioSpec obtained;

        _deviceId = SDL.SDL_OpenAudioDevice(null, 0, ref desiredSpec, out obtained,
            (int)SDL.SDL_AUDIO_ALLOW_FREQUENCY_CHANGE);

        if (_deviceId == 0)
        {
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

    public override void SetPlaybackSpeed(float speed)
    {
        _playbackSpeed = speed;
    }
    
    private float _playbackSpeed = 1.0f;

    public void Callback(IntPtr userdata, IntPtr stream, int len)
    {
        SDL.SDL_LockAudioDevice(_deviceId);

        // Convert the stream pointer to a managed array
        int length = len / sizeof(short);
        short[] audioStream = new short[length];
        Marshal.Copy(stream, audioStream, 0, length);

        int size = AudioBuffer.Count;

        // Assuming playbackSpeed is a float value controlling the playback speed
        // Calculate the number of samples to read based on the playback speed
        int resampleSize = (int)(size / _playbackSpeed);
        if (resampleSize > length)
            resampleSize = length;

        short[] resampledAudio = new short[resampleSize];

        short lastValue = 0;
        // Resample the audio data
        for (int i = 0; i < resampleSize; i++)
        {
            float srcIndex = i * _playbackSpeed;
            int index = (int)srcIndex;
            float frac = srcIndex - index;

            if (index + 1 < size)
            {
                // Linear interpolation
                resampledAudio[i] = (short)((1 - frac) * AudioBuffer[index] + frac * AudioBuffer[index + 1]);
            }
            else
            {
                resampledAudio[i] = AudioBuffer[index];
            }
            lastValue = resampledAudio[i];
        }

        // Fill the rest with the last known value
        if (resampleSize < length)
        {
            Array.Resize(ref resampledAudio, length);
            for (int i = resampleSize; i < length; i++)
            {
                resampledAudio[i] = lastValue;
            }
        }

        Marshal.Copy(resampledAudio, 0, stream, length);

        // Remove the used samples from the buffer
        int samplesUsed = (int)(resampleSize * _playbackSpeed);
        if (samplesUsed > size)
        {
            samplesUsed = size;
        }
        AudioBuffer.RemoveRange(0, samplesUsed);

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
        if (SyncToAudio)
        {
            //         while ((AudioBuffer.Count >> 1) > BufferSize)
            //    SDL.SDL_Delay(1);
        }

        AudioBuffer.Add(left);
        AudioBuffer.Add(right);
    }
}