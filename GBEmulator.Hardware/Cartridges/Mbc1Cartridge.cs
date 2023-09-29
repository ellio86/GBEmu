namespace GBEmulator.Hardware.Cartridges;

using Core.Interfaces;
internal class Mbc1Cartridge : ICartridge
{
    /// <summary>
    /// Bytes from .gb rom file
    /// </summary>
    private byte[] _rom = new byte[8 * 1024 * 1024];
    
    /// <summary>
    /// Represents the 4 banks of external memory.
    /// <list type="bullet">
    /// <item>Bank 0: 0x0000 - 0x1FFF </item>
    /// <item>Bank 2: 0x2000 - 0x3FFF </item>
    /// <item>Bank 3: 0x4000 - 0x5FFF </item>
    /// <item>Bank 4: 0x6000 - 0x7FFF </item>
    /// </list>
    /// </summary>
    private byte[] _externalMemory = new byte [0x8000];
    byte[] ICartridge.ExternalMemoryBytes => _externalMemory;

    private bool _externalMemoryEnabled = false;
    private int _currentRomBank = 1;
    private bool _usingRAMBanks = false;
    private int _currentRamBank = 0;

    public Mbc1Cartridge(string path)
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
        return _externalMemory[0x2000 * _currentRamBank + (address & 0x1FFF)];
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
            _externalMemory[0x2000 * _currentRamBank + (address & 0x1FFF)] = value;
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
                _currentRomBank = value & 0x1F;
                if (_currentRomBank == 0x00 || _currentRomBank == 0x20 || _currentRomBank == 0x40 || _currentRomBank == 0x60)
                {
                    _currentRomBank++;
                }
                break;
            
            // Writing 0 - 3 to 0x4000 - 0x5FFF selects the Upper ROM/RAM bank 
            case < 0x6000:
                if (_usingRAMBanks)
                {
                    _currentRamBank = value & 0b11;
                    break;
                }
                
                // Using ROM banks
                _currentRomBank |= value & 0b11;
                if (_currentRomBank == 0x00 || _currentRomBank == 0x20 || _currentRomBank == 0x40 || _currentRomBank == 0x60)
                {
                    _currentRomBank++;
                }
                break;
            
            // Writing 0 - 1 to 0x6000 - 0x8000 selects whether the above case points at ROM or RAM banks
            case <= 0x8000:
                _usingRAMBanks = value == 0x01;
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