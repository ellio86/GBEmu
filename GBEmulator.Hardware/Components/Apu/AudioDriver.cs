using GBEmulator.Core.Interfaces;

namespace GBEmulator.Hardware.Components.Apu;

public class AudioDriver : IAudioDriver
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
    
    public int Start(uint sampleRate, uint bufferSize)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public void Pause(bool value)
    {
        throw new NotImplementedException();
    }

    public void AddSample(float left, float right)
    {
        left *= _volume;
        right *= _volume;
        
        // TODO: Add sample to the audio sdl 
        //InternalAddSample(left, right);
    }

    public void SetSyncToAudio(bool syncToAudio)
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

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

    public uint SampleRate { get; set; }
    public uint BufferSize { get; set; }
    public bool SyncToAudio { get; set; }
    public List<int> AudioBuffer { get; set; }
}