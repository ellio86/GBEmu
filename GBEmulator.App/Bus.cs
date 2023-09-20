namespace GBEmulator.App;
using Core.Interfaces;

public class Bus : IBus
{
    private readonly ICpu _cpu;
    private readonly byte[] _memory = new byte[1024 * 64];
    private readonly byte[] _rom = new byte[1024 * 32];
    private readonly string _romPath;
    public bool CartridgeLoaded => !string.IsNullOrEmpty(_romPath);

    public Bus(ICpu cpu, string romPath = "")
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        _cpu.ConnectToBus(this);
        _romPath = romPath;
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

    public void Reset()
    {
        // Clear memory
        for (var i = 0; i < _memory.Length; i++) { _memory[i] = 0x00; }
    }

    public void ReadRom()
    {
        using var stream = File.Open(_romPath, FileMode.Open);

        stream.Read(_rom, 0, 32 * 1024);

        for (var i = 0; i < _rom.Length; i++)
        {
            WriteMemory((ushort)i, _rom[i]);
        }
    }
}
