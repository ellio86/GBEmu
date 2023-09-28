namespace GBEmulator.Core.Interfaces;

public interface ICartridge
{
    public byte ReadExternalMemory(ushort address);
    public byte ReadRom(ushort address);
    public byte ReadUpperRom(ushort address);
    public void WriteToRom(ushort address, byte value);
    public void WriteExternalMemory(ushort address, byte value);
}