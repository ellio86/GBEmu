namespace GBEmulator.Core.Interfaces;

public interface IAudioDriver
{
    public int Start(uint sampleRate, uint bufferSize);
    public void Stop();
    public void Pause(bool value);
    public void AddSample(float left, float right);
    public void SetSyncToAudio(bool syncToAudio);
    public void Reset();
    
    public uint GetSampleRate();
    public uint GetBufferSize();

    public bool GetSyncToAudio() ;

    // Volume is a number ranging from 0 to 100
    public int GetVolume();
    public void AddToVolume(int amount);
    
   // protected void InternalAddSample(float left, float right) ;
    protected uint SampleRate { get; set; }
    protected uint BufferSize { get; set; }
    protected bool SyncToAudio { get; set; }
    protected List<int> AudioBuffer { get; set; }
}