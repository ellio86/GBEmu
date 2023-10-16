namespace GBEmulator.Core.Interfaces;

using Enums;
public interface IPpu : IHardwareComponent
{
    public void Clock(int numberOfCycles);
    public PpuMode CurrentMode { get; }
}