namespace GBEmulator.Hardware.Cartridges;

using Core.Interfaces;


public class Cartridge : ICartridge
{
    private readonly ICartridge _cartridge;
    private readonly string _romPath;
    private string GameName => Path.GetFileName(_romPath).Replace(".gb", "");

    /// <summary>
    /// Byte codes for cartridge types with batteries - i.e. saving is enabled. Determined by 0x0147 in the cartridge header
    /// </summary>
    public static byte[] CartridgesWithSavesEnabled = { 0x03, 0x06, 0x09, 0x0D, 0x0F, 0x10, 0x13, 0x1B, 0x1E, 0x22, 0xFF };

    public Cartridge(string fileToLoad, string? saveDirectory)
    {
        _romPath = fileToLoad;
        _cartridge = CreateCartridge(fileToLoad);

        if (!_cartridge.SavesEnabled) return;

        // Try to load save file if cartridge supports saving
        var saveLocation = Path.Join(saveDirectory, $"{GameName}.sav");
        if (Path.Exists(saveLocation))
        {
            _cartridge.LoadSaveFile(saveLocation);
        }
    }

    public byte ReadExternalMemory(ushort address) => _cartridge.ReadExternalMemory(address);
    public byte ReadRom(ushort address) => _cartridge.ReadRom(address);
    public byte ReadUpperRom(ushort address) => _cartridge.ReadUpperRom(address);
    public void WriteToRom(ushort address, byte value) => _cartridge.WriteToRom(address, value);
    public void WriteExternalMemory(ushort address, byte value) => _cartridge.WriteExternalMemory(address, value);
    public bool SavesEnabled => _cartridge.SavesEnabled;
    public void EnableSaves() => _cartridge.EnableSaves();
    public void LoadSaveFile(string path) => _cartridge.LoadSaveFile(path);
    byte[] ICartridge.ExternalMemoryBytes => _cartridge.ExternalMemoryBytes;
    string ICartridge.GameName => GameName;

    private static ICartridge CreateCartridge(string fileToLoad)
    {
        var bytes = File.ReadAllBytes(fileToLoad);
        var cartridgeTypeFromCartridgeHeader = bytes[0x0147];

        ICartridge cartridge = cartridgeTypeFromCartridgeHeader switch
        {
            0x00 => new Mbc0Cartridge(fileToLoad),
            0x01 or 0x02 or 0x03 => new Mbc1Cartridge(fileToLoad),
            0x0F or 0x10 or 0x011 or 0x12 or 0x13 => new Mbc3Cartridge(fileToLoad),
            _ => throw new NotImplementedException(Convert.ToString(bytes[0x0147], 16))
        };

        if (CartridgesWithSavesEnabled.Contains(cartridgeTypeFromCartridgeHeader))
        {
            cartridge.EnableSaves();
        }
        return cartridge;
    }
}