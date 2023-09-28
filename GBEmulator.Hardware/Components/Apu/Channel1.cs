namespace GBEmulator.Hardware.Components.Apu;

using Core.Interfaces;

public class Channel1 : ISoundChannel
{
    public IDac Dac { get; }
    public byte SweepRegister { get; set; }
    public byte LengthTimerDutyCycleRegister { get; set; }
    public byte VolumeAndEnvelopeRegister { get; set; }
    public byte PeriodLow { get; set; }
    public byte PeriodHigh { get; set; }
    
    public int Update(int numberOfTicks)
    {
        throw new NotImplementedException();
    }
}