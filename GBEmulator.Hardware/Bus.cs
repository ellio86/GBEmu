namespace GBEmulator.Hardware;
using Core.Interfaces;
using Core.Enums;

public class Bus : IBus
{
    // Hardware components connected to bus
    private readonly ICpu _cpu;
    private readonly ITimer _timer;
    private readonly IPpu _ppu;
    
    // Memory
    private readonly byte[] _memory = new byte[1024 * 64];
    private readonly byte[] _rom = new byte[1024 * 32];
    
    // ROM loaded?
    public bool CartridgeLoaded { get; private set; } = false;
    
    /// <summary>
    /// Handle interrupt request
    /// </summary>
    /// <param name="interruptRequest"></param>
    public void Interrupt(Interrupt interruptRequest)
    {
        _cpu.Interrupt(interruptRequest);
    }

    public Bus(ICpu cpu, ITimer timer, IPpu ppu)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        _timer = timer ?? throw new ArgumentNullException(nameof(timer));
        _ppu = ppu ?? throw new ArgumentNullException(nameof(ppu));
        _cpu.ConnectToBus(this);
        _timer.ConnectToBus(this);
        _ppu.ConnectToBus(this);
        Reset();
    }

    public byte ReadMemory(ushort address)
    {
        if (address == 0xFF4D)
        {
            return 0xFF;
        }
        return _memory[address];
    }

    public void WriteMemory(ushort address, byte value)
    {
        _memory[address] = value;
    }

    /// <summary>
    /// See https://gbdev.io/pandocs/Power_Up_Sequence.html
    /// </summary>
    public void Reset()
    {
        _cpu.Reset();
        
        // Clear memory
        for (var i = 0; i < _memory.Length; i++) { _memory[i] = 0x00; }

        // Set Hardware Registers
        WriteMemory((ushort)HardwareRegisters.P1, 0xCF);
        WriteMemory((ushort)HardwareRegisters.SC, 0x7E);
        WriteMemory((ushort)HardwareRegisters.DIV, 0xAB);
        WriteMemory((ushort)HardwareRegisters.TAC, 0xF8);
        WriteMemory((ushort)HardwareRegisters.IF, 0xE1);
        WriteMemory((ushort)HardwareRegisters.NR10, 0x80);
        WriteMemory((ushort)HardwareRegisters.NR11, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR12, 0xF3);
        WriteMemory((ushort)HardwareRegisters.NR13, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR14, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR21, 0x3F);
        WriteMemory((ushort)HardwareRegisters.NR23, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR24, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR30, 0x7F);
        WriteMemory((ushort)HardwareRegisters.NR31, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR32, 0x9F);
        WriteMemory((ushort)HardwareRegisters.NR33, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR33, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR41, 0xFF);
        WriteMemory((ushort)HardwareRegisters.NR44, 0xBF);
        WriteMemory((ushort)HardwareRegisters.NR50, 0x77);
        WriteMemory((ushort)HardwareRegisters.NR51, 0xF3);
        WriteMemory((ushort)HardwareRegisters.NR51, 0xF3);
        WriteMemory((ushort)HardwareRegisters.NR52, 0xF1);
        WriteMemory((ushort)HardwareRegisters.LCDC, 0x91);
        WriteMemory((ushort)HardwareRegisters.STAT, 0x85);
        WriteMemory((ushort)HardwareRegisters.DMA, 0xFF);
        WriteMemory((ushort)HardwareRegisters.DMA, 0xFF);
        WriteMemory((ushort)HardwareRegisters.BGP, 0xFC);
        WriteMemory((ushort)HardwareRegisters.KEY1, 0xFF);
        WriteMemory((ushort)HardwareRegisters.VBK, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA1, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA2, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA3, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA4, 0xFF);
        WriteMemory((ushort)HardwareRegisters.HDMA5, 0xFF);
        WriteMemory((ushort)HardwareRegisters.RP, 0xFF);
        WriteMemory((ushort)HardwareRegisters.BCPS, 0xFF);
        WriteMemory((ushort)HardwareRegisters.BCPD, 0xFF);
        WriteMemory((ushort)HardwareRegisters.OCPS, 0xFF);
        WriteMemory((ushort)HardwareRegisters.OCPD, 0xFF);
        WriteMemory((ushort)HardwareRegisters.SVBK, 0xFF);

        //WriteMemory((ushort) 0xFF44, 0x90);
    }

    public void LoadRom(string path)
    {
        using var stream = File.Open(path, FileMode.Open);

        stream.Read(_rom, 0, 32 * 1024);

        for (var i = 0; i < _rom.Length; i++)
        {
            WriteMemory((ushort)i, _rom[i]);
        }

        CartridgeLoaded = true;
    }
}
