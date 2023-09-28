namespace GBEmulator.Hardware;

public class Cartridge : ICartridge
{
    private ICartridge _cartridge;
    public Cartridge(string fileToLoad) 
    {
        _cartridge = CreateCartridge(fileToLoad);
    }

    public byte ReadExternalMemory(ushort address) => _cartridge.ReadExternalMemory(address);
    public byte ReadRom(ushort address) => _cartridge.ReadRom(address);
    public byte ReadUpperRom(ushort address) => _cartridge.ReadUpperRom(address);
    public void WriteToRom(ushort address, byte value) => _cartridge.WriteToRom(address, value);
    public void WriteExternalMemory(ushort address, byte value) => _cartridge.WriteExternalMemory(address, value);

    private static ICartridge CreateCartridge(string fileToLoad)
    {
        var bytes = File.ReadAllBytes(fileToLoad);

        ICartridge cartridge = bytes[0x0147] switch
        {
            0x00 => new Mbc0Cartridge(fileToLoad),
            0x01 or 0x02 or 0x03 => new Mbc1Cartridge(fileToLoad),
            0x0F or 0x10 or 0x011 or 0x12 or 0x13 => new Mbc3Cartridge(fileToLoad),
            _ => throw new NotImplementedException(Convert.ToString(bytes[0x0147], 16))
        };

        return cartridge;
    }
}