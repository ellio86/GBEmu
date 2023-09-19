namespace GBEmulator.Core;
using System;

public class Cpu : ICpu
{
    public IRegisters _registers;

    public Cpu(IRegisters registers)
    {
        _registers = registers ?? throw new ArgumentNullException(nameof(registers));
    }
}
