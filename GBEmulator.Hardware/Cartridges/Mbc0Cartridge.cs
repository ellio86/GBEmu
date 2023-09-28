namespace GBEmulator.Hardware.Cartridges;

using Core.Interfaces;

public class Mbc0Cartridge : ICartridge
{
    /// <summary>
    /// Bytes from .gb rom file
    /// </summary>
    private byte[] _rom = new byte[1024 * 32];
    
    public Mbc0Cartridge(string path)
    {
        LoadRomFromPath(path);
    }

    private void LoadRomFromPath(string path)
    {
        var bytes =  File.ReadAllBytes(path);
        for (var i = 0; i < bytes.Length; i++)
        {
            _rom[i] = bytes[i];
        }
    }

    public byte ReadExternalMemory(ushort address)
    {
        return 0xFF;
    }

    public byte ReadRom(ushort address)
    {
        return _rom[address];
    }

    public byte ReadUpperRom(ushort address)
    {
        return _rom[address];
    }

    public void WriteExternalMemory(ushort address, byte value)
    {

    }
    
    public void WriteToRom(ushort address, byte value)
    {
        
    }
}