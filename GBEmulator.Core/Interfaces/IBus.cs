namespace GBEmulator.Core.Interfaces;

using System.Drawing;
using Enums;
using Interfaces;

public interface IBus
{
    /// <summary>
    /// Reads the address from the attached memory
    /// </summary>
    /// <param name="address">Address to read from.</param>
    /// <param name="consumesCycles">Whether the components in the bus other than the cpu get ticked</param>
    /// <returns></returns>
    public byte ReadMemory(ushort address, bool consumesCycles = true);

    /// <summary>
    /// Writes the provided value to the provided address in memory
    /// </summary>
    /// <param name="address">Address to write to</param>
    /// <param name="value">Value to set</param>
    /// <param name="consumesCycles">Whether the components in the bus other than the cpu get ticked</param>
    public void WriteMemory(ushort address, byte value, bool consumesCycles = true);

    /// <summary>
    /// Clears memory and resets hardware registers. See https://gbdev.io/pandocs/Power_Up_Sequence.html
    /// </summary>
    public void Reset();

    /// <summary>
    /// Is a ROM loaded in the GameBoy's memory?
    /// </summary>
    public bool CartridgeLoaded { get; }
    
    /// <summary>
    /// Handle interrupt request
    /// </summary>
    /// <param name="interruptRequest"></param>
    public void Interrupt(Interrupt interruptRequest);

    public void FlipWindow();

    public void SetBitmap(Bitmap bmp);
    
    public void LoadCartridge(ICartridge cartridgeToLoad);
    public int ClockCpu(StreamWriter logWriter);
    public void ClockPpu(int cycleNum);
    public void ClockTimer(int cycleNum);
    public void HandleInterrupts();
    public IRegisters GetCpuRegisters();
    public void TickComponents();
    public void DumpExternalMemory(string name);
}
