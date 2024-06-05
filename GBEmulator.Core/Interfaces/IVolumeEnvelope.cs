namespace GBEmulator.Core.Interfaces;

public interface IVolumeEnvelope
{
    public void Step();
    public void PowerOff();
    public void SetNr2(byte value);
    public byte GetNr2();
    public byte GetVolume();
    public void Trigger();
}