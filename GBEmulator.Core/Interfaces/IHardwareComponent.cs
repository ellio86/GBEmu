namespace GBEmulator.Core.Interfaces;

public interface IHardwareComponent
{
    /// <summary>
    /// Connects this hardware component to the provided BUS
    /// </summary>
    /// <param name="bus">Bus to connect component to</param>
    public void ConnectToBus(IBus bus);
}