using System.Threading.Channels;
using GBEmulator.Core.Interfaces;
using GBEmulator.Core.Models;

namespace GBEmulator.Hardware.Components.Apu;

public abstract class Channel : HardwareComponent, IChannel
{
    public abstract void Tick();
    public abstract void PowerOff();
    public abstract byte Read(ushort address);
    public abstract void Write(ushort address, byte value);
    
    public ILengthCounter LengthCounter { get; set; } = new LengthCounter();
    public bool ChannelEnabled { get; set; }
    public bool DacEnabled { get; set; }
    public byte Output { get; set; }

    public Channel()
    {
        ChannelEnabled = false;
        DacEnabled = false;
        Output = 0;
    }

    public byte GetOutput()
    {
        return Output;
    }
    
    public bool IsEnabled()
    {
        return ChannelEnabled && DacEnabled;
    }

    public void LengthClock()
    {
        LengthCounter.Step();

        if (LengthCounter.IsEnabled() && LengthCounter.IsZero())
            ChannelEnabled = false;
    }

    public virtual void SweepClock()
    {
    }

    public virtual void EnvelopeClock()
    {
    }

    public void SetFrameSequencer(int frameSequencer)
    {
        LengthCounter.SetFrameSequencer(frameSequencer);
    }
}