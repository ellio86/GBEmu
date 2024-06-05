namespace GBEmulator.Core.Interfaces;

public interface IFrequencySweep
{
    public void Step();
    public bool IsEnabled();
    public void PowerOff();
    public void SetNr10(byte value);
    public void SetNr13(byte value);
    public void SetNr14(byte value);
    public byte GetNr10();

    public int GetFrequency();
    public void Trigger();
}