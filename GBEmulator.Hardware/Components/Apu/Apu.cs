namespace GBEmulator.Hardware.Components.Apu;

using Core.Interfaces;
using Core.Models;

public class Apu : HardwareComponent, IApu
{
    public byte SoundControl { get; set; }
    public byte SoundPanning { get; set; }
    public byte MasterVolVinPanning { get; set; }
    public ISoundChannel Channel1 { get; }
    public ISoundChannel Channel2 { get; }
    public ISoundChannel Channel3 { get; }
    public ISoundChannel Channel4 { get; }
    
    public IMixer Mixer { get; }

    public Apu(IMixer mixer)
    {
        Mixer = mixer ?? throw new ArgumentNullException(nameof(mixer));
    }
    
    public void TriggerChannel(int channelNumber)
    {
        throw new NotImplementedException();
    }

    public void Update(int tickNum)
    {
        var channel1Output = Channel1.Update(tickNum);
    } 
}