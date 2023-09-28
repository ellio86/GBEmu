namespace GBEmulator.Core.Interfaces;

public interface IApu : IHardwareComponent
{
    /// <summary>
    /// 0xFF26 - Register NR52: Sound Control Register 
    /// </summary>
    /// <remarks>
    ///  Writing to bits 0, 1, 2, or 3 DOES NOT enable/disable the channels, they are READ ONLY
    /// </remarks>
    public byte SoundControl { get; set; }
    
    /// <summary>
    /// 0xFF25 - Register NR51: Sound Panning Register
    /// </summary>
    public byte SoundPanning { get; set; }
    
    /// <summary>
    /// 0xFF24 - Register NR50: Master Volume and VIN Panning
    /// </summary>
    public byte MasterVolVinPanning { get; set; }
    
    /// <summary>
    /// Causes the selected channel to play its wave from the beginning
    /// </summary>
    /// <param name="channelNumber"></param>
    public void TriggerChannel(int channelNumber);
    
    // Sound channels
    public ISoundChannel Channel1 { get; }
    public ISoundChannel Channel2 { get; }
    public ISoundChannel Channel3 { get; }
    public ISoundChannel Channel4 { get; }
}