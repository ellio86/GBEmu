namespace GBEmulator.Core.Interfaces;

public interface ITimer : IHardwareComponent
{
    public void Clock(int numOfCycles);
}