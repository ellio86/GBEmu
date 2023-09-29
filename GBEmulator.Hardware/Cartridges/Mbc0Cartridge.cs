namespace GBEmulator.Hardware.Cartridges;

using Core.Interfaces;

/// <summary>
/// Should not be directly instantiated. Should be accessed through <see cref="Cartridge"/>.
/// </summary>
internal class Mbc0Cartridge : ICartridge
{
    /// <summary>
    /// Bytes from .gb rom file
    /// </summary>
    private byte[] _rom = new byte[1024 * 32];

    byte[] ICartridge.ExternalMemoryBytes => Array.Empty<byte>();

    public Mbc0Cartridge(string path)
    {
        LoadRomFromPath(path);
    }

    private void LoadRomFromPath(string path)
    {
        var bytes = File.ReadAllBytes(path);
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

    bool ICartridge.SavesEnabled => false;

    public void EnableSaves()
    {
        throw new InvalidOperationException(
            "MBC0 Cartridges do not have batteries and therefore do not support saving.");
    }

    public void WriteToRom(ushort address, byte value)
    {
        // No rom to write to, do nothing here.
    }

    public void WriteExternalMemory(ushort address, byte value)
    {
        // No external memory to write to, do nothing here.
    }

    public void LoadSaveFile(string path)
    {
        // Nowhere to load save file to, ignored
    }

    public string GameName => throw new InvalidOperationException("Cartridge Game Name accessed directly! Please access through Cartridge class/");
}