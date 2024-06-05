using System.Runtime.InteropServices;
using GBEmulator.Core.Interfaces;
using SDL2;


namespace GBEmulator.Hardware.Components.Apu;

public class AudioSdl : AudioDriver
{
    private bool _started { get; set; }
    private uint _deviceId { get; set; }
    private SDL.SDL_AudioCallback _callback; 

    public AudioSdl() : base()
    {
        _started = false;
    }
    
    public override int Start(uint sampleRate, uint bufferSize)
    {
        SampleRate = sampleRate;
        BufferSize = bufferSize;
        
        // Initialise SDL audio
        

        if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) < 0) {
            return -1;
        }
        else {
            //LOG_DEBUG("Initialized audio subsystem");
        }
        _callback = new SDL.SDL_AudioCallback(Callback);
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
            //LOG_ERROR("Unable to open audio-device: " + std::string(SDL_GetError()));
            return -1;
        }
        else
        {
            //LOG_DEBUG("Opened audio-device");
        }

        SDL.SDL_PauseAudioDevice(_deviceId, 0);
        _started = true;

        return 0;
    }

    public void Callback(IntPtr userdata, IntPtr stream, int len)
    {
        var audioStream = new int[len / sizeof(short)];
        len >>= 1;

        int size = AudioBuffer.Count;

        if (size > len)
            size = len;
        else if (size < len)
            Array.Fill(audioStream, size, len - size, 0);

        AudioBuffer.CopyTo(0, audioStream, 0, size);
        AudioBuffer.RemoveRange(0, size);

        Marshal.Copy(audioStream, 0, stream, len);
    }

    public override void Stop()
    {
        if (!_started)
            return;

        _started = false;

        SDL.SDL_PauseAudioDevice(_deviceId, 1);
        SDL.SDL_CloseAudioDevice(_deviceId);

        SDL.SDL_QuitSubSystem(SDL.SDL_INIT_AUDIO);

        //LOG_DEBUG("Stopped audio-device");
    }

    public override void Pause(bool value)
    {
        SDL.SDL_PauseAudioDevice(_deviceId, value ? 1 : 0);
    }

    public override void SetSyncToAudio(bool syncToAudio)
    {
        SDL.SDL_LockAudioDevice(_deviceId);

        AudioBuffer = new List<int>();
        SyncToAudio = syncToAudio;

        SDL.SDL_UnlockAudioDevice(_deviceId);
    }
    
    public override void Reset()
    {
        SDL.SDL_LockAudioDevice(_deviceId);
        AudioBuffer = new List<int>();
        SDL.SDL_UnlockAudioDevice(_deviceId);
    }

    public override void InternalAddSample(int left, int right)
    {
       // if (SyncToAudio) {
       //     while ((AudioBuffer.Count >> 1) > BufferSize)
       //         SDL.SDL_Delay(1);
       // }

        AudioBuffer.Add(left);
        AudioBuffer.Add(right);
    }
}