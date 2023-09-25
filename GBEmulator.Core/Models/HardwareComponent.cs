namespace GBEmulator.Core.Models;

using Interfaces;

public abstract class HardwareComponent
{
    protected IBus _bus = null!;
    
    public void ConnectToBus(IBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }
}