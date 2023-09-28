namespace GBEmulator.Core.Interfaces;

public interface IController : IHardwareComponent
{
    public void HandleKeyDown(byte keyBit);
    public void HandleKeyUp(byte keyBit);
    public void Update();
}