namespace GBEmulator.Core.Interfaces;

public interface IChannel : IHardwareComponent
{
    public void Tick();
    public void PowerOff();

    public byte GetOutput();
    public byte Read(ushort address);
    public void Write(ushort address, byte value);
    public bool IsEnabled();

    public void LengthClock();
    public void SweepClock();
    public void EnvelopeClock();

    public void SetFrameSequencer(int frameSequencer);

    protected ILengthCounter LengthCounter { get; set; }
    protected bool ChannelEnabled { get; set; }
    protected bool DacEnabled { get; set; }
    protected byte Output { get; set; }

}