using GBEmulator.Core.Interfaces;

namespace GBEmulator.Hardware.Components.Apu;

public class LengthCounter: ILengthCounter
{
    public void Step()
    {
        throw new NotImplementedException();
    }

    public void SetNr4(byte value)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled()
    {
        throw new NotImplementedException();
    }

    public bool IsZero()
    {
        throw new NotImplementedException();
    }

    public void SetLength(byte length)
    {
        throw new NotImplementedException();
    }

    public void SetFullLength(int fullLength)
    {
        throw new NotImplementedException();
    }

    public void PowerOff(bool gbcMode)
    {
        throw new NotImplementedException();
    }

    public void SetFrameSequencer(int frameSequencer)
    {
        throw new NotImplementedException();
    }
}