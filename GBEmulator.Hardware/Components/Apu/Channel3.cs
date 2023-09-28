namespace GBEmulator.Hardware.Components.Apu;
using Core.Models;
using Core.Interfaces;
using Core.Enums;

public class Channel3 : HardwareComponent, ISoundChannel
{
    public IDac Dac { get; }
    public byte SweepRegister { get; set; }
    public byte LengthTimerDutyCycleRegister { get; set; }
    public byte VolumeAndEnvelopeRegister { get; set; }
    public byte PeriodLow { get; set; }
    public byte PeriodHigh { get; set; }
    bool ISoundChannel.DacEnabled => (_bus.ReadMemory((ushort)HardwareRegisters.NR30) & 0b10000000) > 0;
    
    public int Update(int numberOfTicks)
    {
        throw new NotImplementedException();
    }
}