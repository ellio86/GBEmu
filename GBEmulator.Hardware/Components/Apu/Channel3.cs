using System.Threading.Channels;
using GBEmulator.Core.Interfaces;
using GBEmulator.Core.Models;

namespace GBEmulator.Hardware.Components.Apu;

public class Channel3 : Channel
{
    private byte[] _waveTable = new byte[16];
    private int _timer;
    private int _position;
    private int _ticksSinceRead;
    private int _lastAddress;
    private int _frequency;
    private byte _volumeCode;

    public Channel3() 
    {
        _timer = 0;
        _position = 4;
        _ticksSinceRead = 0;
        _frequency = 0;
        _volumeCode = 0;
        _lastAddress = 0;

        LengthCounter.SetFullLength(256);
    }

    private void Trigger()
    {
        // Triggering while it reads a sample byte corrupts the data
        if (IsEnabled() && _timer == 2) {
            int pos = _position >> 1;

            if (pos < 4)
                _waveTable[0] = _waveTable[pos];
            else {
                pos &= ~0b11; // Make it 4-byte aligned
                Array.Copy(_waveTable, pos, _waveTable, 0, 4);
                //std::copy(&wave_table[pos], &wave_table[pos+4], &wave_table[0]);
            }
        }

        _timer = 6; // Note: You need this to get blargg's "09-wave read while on" test

        _position = 0;
        _lastAddress = 0;

        ChannelEnabled = DacEnabled;
    }
    
    public override void Tick()
    {
        _ticksSinceRead++;

        if (--_timer <= 0) {
            _timer = (2048 - _frequency) << 1;

            if (IsEnabled()) {
                _ticksSinceRead = 0;

                _lastAddress = _position >> 1;
                Output = _waveTable[_lastAddress];

                if ((_position & 1) > 0)
                    Output &= 0x0F;
                else
                    Output >>= 4;

                if (_volumeCode > 0)
                    Output >>= (_volumeCode - 1);
                else
                    Output = 0;

                _position = (_position + 1) & 31;
            }
            else
                Output = 0;
        }
    }

    public override void PowerOff()
    {
        LengthCounter.PowerOff(false);

        ChannelEnabled = false;
        DacEnabled  = false;

        _position = 0;
        _frequency = 0;
        _volumeCode = 0;

        _ticksSinceRead = 0;
        _lastAddress = 0;
    }

    public override byte Read(ushort address)
    {
        if (address >= 0xFF30 && address <= 0xFF3F) {
            if (IsEnabled()) {
                if (_ticksSinceRead < 2)
                    return _waveTable[_lastAddress];
                else
                    return 0xFF;
            }
            else
                return _waveTable[address - 0xFF30];
        }

        switch (address) {
            case 0xFF1A:
                return (byte)((DacEnabled ? 0x80 : 0) | 0x7F);

            case 0xFF1B:
                return 0xFF;

            case 0xFF1C:
                return (byte)((_volumeCode << 5) | 0x9F);

            case 0xFF1D:
                return 0xFF;

            case 0xFF1E:
                return (byte)((LengthCounter.IsEnabled() ? 0x40 : 0) | 0xBF);

            default:
                break;
        }
        
        // Can't access provided address
        return 0;
    }

    public override void Write(ushort address, byte value)
    {
        if (address >= 0xFF30 && address <= 0xFF3F) {
            if (IsEnabled()) {
                if (_ticksSinceRead < 2)
                    _waveTable[_lastAddress] = value;
            }
            else
                _waveTable[address - 0xFF30] = value;
            return;
        }

        switch (address) {
            case 0xFF1A:
                DacEnabled = (value & 0x80) != 0;
                ChannelEnabled &= DacEnabled;
                return;

            case 0xFF1B:
                LengthCounter.SetLength(value);
                return;

            case 0xFF1C:
                _volumeCode = (byte)((value >> 5) & 0b11);
                return;

            case 0xFF1D:
                _frequency = (_frequency & 0x700) | value;
                return;

            case 0xFF1E:
                LengthCounter.SetNr4(value);
                _frequency = (_frequency & 0xFF) | ((value & 0b111) << 8);

                if (LengthCounter.IsEnabled() && LengthCounter.IsZero())
                    ChannelEnabled = false;
                else if ((value & 0x80) > 0)
                    Trigger();
                return;
        }

    }
}