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
        _callback = Callback;
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
        // Calculate the number of samples in the stream (each sample is 2 bytes)
        var samples = len / sizeof(short);
        var audioStream = new int[samples];

        // Calculate the number of samples available in the AudioBuffer
        var availableSamples = AudioBuffer.Count;

        // Determine the number of samples to process for pitch doubling
        var processSamples = Math.Min(samples * 2, availableSamples);

        var tempBuffer = new int[processSamples];

        // Copy available samples to the temporary buffer
        AudioBuffer.CopyTo(0, tempBuffer, 0, processSamples);
        AudioBuffer.RemoveRange(0, processSamples);

        // Process samples to double the pitch
        for (int i = 0, j = 0; i < processSamples && j < samples; i += 2, j++)
        {
            audioStream[j] = tempBuffer[i];
        }

        // If we have fewer samples than needed, interpolate
        if (processSamples < samples * 2)
        {
            int lastSampleIndex = processSamples / 2;

            for (var i = lastSampleIndex; i < samples; i++)
            {
                if (i < lastSampleIndex - 1)
                {
                    // Linear interpolation between the last two available samples
                    audioStream[i] = (audioStream[lastSampleIndex - 1] + audioStream[lastSampleIndex - 2]) / 2;
                }
                else
                {
                    // If we are at the edge, just copy the last available sample
                    audioStream[i] = audioStream[lastSampleIndex - 1];
                }
            }
        }

        // Copy the processed samples to the output stream
        Marshal.Copy(audioStream, 0, stream, samples);
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