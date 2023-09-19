namespace GBEmulator.App;
using Core.Interfaces;

public class Bus : IBus
{
    private readonly ICpu _cpu;
    private readonly byte[] _memory = new byte[1024 * 8];
    public Bus(ICpu cpu)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        _cpu.ConnectToBus(this);

        // Clear memory
        for(var i = 0; i < _memory.Length; i++) { _memory[i] = 0x00; }
    }

    public byte ReadMemory(ushort address)
    {
        return _memory[address];
    }

    public void WriteMemory(ushort address, byte value)
    {
        _memory[address] = value;
    }
}
