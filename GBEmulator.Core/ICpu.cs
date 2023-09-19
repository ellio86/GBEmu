namespace GBEmulator.Core;

public interface ICpu
{
    public void ConnectToBus(Bus bus);
    public void Clock();
    public void Reset();
}