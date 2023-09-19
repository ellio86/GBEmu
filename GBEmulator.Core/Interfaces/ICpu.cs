namespace GBEmulator.Core.Interfaces;

public interface ICpu
{
    /// <summary>
    /// Connects this instance of the CPU to a BUS
    /// </summary>
    /// <param name="bus">Bus to connect CPU to</param>
    public void ConnectToBus(IBus bus);

    /// <summary>
    /// Execute one tick of the clock
    /// </summary>
    public void Clock();

    /// <summary>
    /// Reset function - clears memory, flags, registers etc
    /// </summary>
    public void Reset();
}