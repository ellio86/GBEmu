namespace GBEmulator.Core.Interfaces;

public interface IBus
{
    /// <summary>
    /// Reads the address from the attached memory
    /// </summary>
    /// <param name="address">Address to read from.</param>
    /// <returns></returns>
    public byte ReadMemory(ushort address);

    /// <summary>
    /// Writes the provided value to the provided address in memory
    /// </summary>
    /// <param name="address">Address to write to</param>
    /// <param name="value">Value to set</param>
    public void WriteMemory(ushort address, byte value);

    /// <summary>
    /// Clears memory
    /// </summary>
    public void Reset();
}
