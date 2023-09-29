namespace GBEmulator.Hardware.Cartridges;

using Core.Interfaces;

internal class Mbc3Cartridge : ICartridge
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
    private int _currentRamBank = 0;

    private byte RTC_S;  //08h  RTC S   Seconds   0-59 (0-3Bh)
    private byte RTC_M;  //09h RTC M Minutes   0-59 (0-3Bh)
    private byte RTC_H;  //0Ah RTC H Hours     0-23 (0-17h)
    private byte RTC_DL; //0Bh RTC DL Lower 8 bits of Day Counter(0-FFh)
    private byte RTC_DH; //0Ch RTC DH Upper 1 bit of Day Counter, Carry Bit, Halt Flag

    public Mbc3Cartridge(string path)
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

        var value = (byte)(_currentRamBank switch
        {
            >= 0x00 and <= 0x03 => _externalMemory[(0x2000 * _currentRamBank) + (address & 0x1FFF)],
            0x08 => RTC_S,
            0x09 => RTC_M,
            0x0A => RTC_H,
            0x0B => RTC_DL,
            0x0C => RTC_DH,
            _ => 0xFF
        });
        return value;
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
            switch (_currentRamBank)
            {
                // First 4 banks are external ram
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                    _externalMemory[0x2000 * _currentRamBank + (address & 0x1FFF)] = value;
                    break;

                // Extra banks are for clock
                case 0x08:
                    RTC_S = value;
                    break;
                case 0x09:
                    RTC_M = value;
                    break;
                case 0x0A:
                    RTC_H = value;
                    break;
                case 0x0B:
                    RTC_DL = value;
                    break;
                case 0x0C:
                    RTC_DH = value;
                    break;
            }
            
        }
    }
    private bool _savesEnabled = false;
    public bool SavesEnabled => _savesEnabled;
    public void EnableSaves()
    {
        _savesEnabled = true;
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
            
            case < 0x4000:
                _currentRomBank = value & 0x7F;
                if (_currentRomBank == 0x00)
                {
                    _currentRomBank++;
                }
                break;
            
            case < 0x6000:
                if (value is >= 0x00 and <= 0x03 || value is >= 0x08 and <= 0xC0)
                {
                    _currentRamBank = value;
                }

                break;

            case <= 0x8000:
                var now = DateTime.Now;
                RTC_S = (byte)now.Second;
                RTC_M = (byte)now.Minute;
                RTC_H = (byte)now.Hour;
                break;
        }
    }
    
    public void LoadSaveFile(string path)
    {
        _externalMemory = File.ReadAllBytes(path);
    }

    public string GameName => throw new InvalidOperationException("Cartridge Game Name accessed directly! Please access through Cartridge class/");
}