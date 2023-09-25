using System.Drawing;
using GBEmulator.Core.Enums;

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
    /// Clears memory and resets hardware registers. See https://gbdev.io/pandocs/Power_Up_Sequence.html
    /// </summary>
    public void Reset();

    /// <summary>
    /// Load rom from provided path into GameBoy's memory
    /// </summary>
    /// <param name="path"></param>
    public void LoadRom(string path);

    /// <summary>
    /// Is a ROM loaded in the GameBoy's memory?
    /// </summary>
    public bool CartridgeLoaded { get; }
    
    /// <summary>
    /// Handle interrupt request
    /// </summary>
    /// <param name="interruptRequest"></param>
    public void Interrupt(Interrupt interruptRequest);

    public void FlipWindow(Bitmap bmp);
}
