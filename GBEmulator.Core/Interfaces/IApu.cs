namespace GBEmulator.Core.Interfaces;

public interface IApu : IHardwareComponent
{
    public void BindAudioDriver(IAudioDriver audioDriver);
    public void Tick();
    public byte Read(ushort address);
    public void Write(ushort address, byte value);
}