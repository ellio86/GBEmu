namespace GBEmulator.Core.Interfaces;

public interface IPpu : IHardwareComponent
{
    public void Clock(int numberOfCycles);
}