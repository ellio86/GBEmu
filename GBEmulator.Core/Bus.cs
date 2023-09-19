namespace GBEmulator.Core;

public class Bus
{
    private readonly ICpu _cpu;
    public Bus(ICpu cpu)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
    }
}
