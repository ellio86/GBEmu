namespace GBEmulator.Hardware.Cartridges;

using Core.Interfaces;
internal class Mbc2Cartridge : ICartridge
{
    /// <summary>
    /// Bytes from .gb rom file
    /// </summary>
    private byte[] _rom = new byte[256 * 1024];
    
    private byte[] _externalMemory = new byte [512]; // Actually 512x4b internal memory for this cartridge
    byte[] ICartridge.ExternalMemoryBytes => _externalMemory;

    private bool _externalMemoryEnabled = false;
    private int _currentRomBank = 1;

    public Mbc2Cartridge(string path)
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
        if (!_externalMemoryEnabled)
        {
            return 0xFF;
        }
        return _externalMemory[address & 0x1FFF];
    }

    public byte ReadRom(ushort address)
    {
        return _rom[address];
    }

    public byte ReadUpperRom(ushort address)
    {
        return _rom[(0x4000 * _currentRomBank) + (address & 0x3FFF)];
    }

    public void WriteExternalMemory(ushort address, byte value)
    {
        if (_externalMemoryEnabled)
        {
            _externalMemory[address & 0x1FFF] = value;
        }
    }

    /// <summary>
    /// If the program tries to write to an address in ROM, it has different behaviours (handled in this function), but
    /// it doesn't actually ever write to the ROM.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    public void WriteToRom(ushort address, byte value)
    {
        switch (address)
        {
            // Writing 0x0A to 0x0000 - 0x1FFF enables the External Memory - this is disabled by default and can only
            // be enabled like this
            case < 0x2000:
                _externalMemoryEnabled = value == 0x0A;
                break;
            
            // Writing 0x01 - 0x1F to 0x2000 - 0x3FFF selects the ROM bank
            case < 0x4000:
                _currentRomBank = value & 0xF;
                break;
        }
    }
    
    private bool _savesEnabled = false;
    public bool SavesEnabled => _savesEnabled;
    public void EnableSaves()
    {
        _savesEnabled = true;
    }

    public void LoadSaveFile(string path)
    {
        _externalMemory = File.ReadAllBytes(path);
    }

    public string GameName => throw new InvalidOperationException("Cartridge Game Name accessed directly! Please access through Cartridge class/");
}