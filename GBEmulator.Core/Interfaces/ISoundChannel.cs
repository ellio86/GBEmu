namespace GBEmulator.Core.Interfaces;

public interface ISoundChannel
{
    public IDac Dac { get; }
    /// <summary>
    /// NRx0
    /// </summary>
    public byte SweepRegister { get; set; }
    
    /// <summary>
    /// NRx1
    /// </summary>
    public byte LengthTimerDutyCycleRegister { get; set; }
    
    /// <summary>
    /// NRx2
    /// </summary>
    public byte VolumeAndEnvelopeRegister { get; set; }
    
    /// <summary>
    /// NRx3
    /// </summary>
    public byte PeriodLow { set; }
    
    /// <summary>
    /// NRx4
    /// </summary>
    public byte PeriodHigh { get; set; }

    public bool DacEnabled => (VolumeAndEnvelopeRegister & 0xF8) != 0;

    public int Update(int numberOfTicks);
}