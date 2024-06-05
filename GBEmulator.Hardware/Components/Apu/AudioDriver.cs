using GBEmulator.Core.Interfaces;

namespace GBEmulator.Hardware.Components.Apu;

public abstract class AudioDriver : IAudioDriver
{
    private void SetVolume(float volume)
    {
        _volume = Math.Clamp(volume, 0, 1f);
    }
    
    private bool _skip;
    private float _volume;

    public AudioDriver()
    {
        _skip = false;
        _volume = 1.0f;
        SyncToAudio = true;
    }

    public abstract int Start(uint sampleRate, uint bufferSize);


    public abstract void Stop();


    public abstract void Pause(bool value);


    public void AddSample(float left, float right)
    {
        left *= _volume;
        right *= _volume;
        
        InternalAddSample((int)left, (int)right);
    }

    public abstract void SetSyncToAudio(bool syncToAudio);
    public abstract void Reset();

    public uint GetSampleRate()
    {
        return SampleRate;
    }

    public uint GetBufferSize()
    {
        return BufferSize;
    }

    public bool GetSyncToAudio()
    {
        return SyncToAudio;
    }

    public int GetVolume()
    {
        return (int)(_volume * 100.0f);
    }

    public void AddToVolume(int amount)
    {
        SetVolume(_volume + amount/100.0f);
    }

    public abstract void InternalAddSample(int left, int right);
    public uint SampleRate { get; set; }
    public uint BufferSize { get; set; }
    public bool SyncToAudio { get; set; }
    public List<int> AudioBuffer { get; set; } = new();
}