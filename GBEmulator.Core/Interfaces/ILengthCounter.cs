namespace GBEmulator.Core.Interfaces;

public interface ILengthCounter
{
    public void Step();
    public void SetNr4(byte value);
    public bool IsEnabled();
    public bool IsZero();
    public void SetLength(byte length);
    public void SetFullLength(int fullLength);
    public void PowerOff(bool gbcMode);
    public void SetFrameSequencer(int frameSequencer);
}