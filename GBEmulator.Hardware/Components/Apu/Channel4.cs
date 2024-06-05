using System.Threading.Channels;
using GBEmulator.Core.Interfaces;
using GBEmulator.Core.Models;

namespace GBEmulator.Hardware.Components.Apu;

public class Channel4 : Channel
{
    private static readonly int[] divisors = new[] { 8, 16, 32, 48, 64, 80, 96, 112 };

    private readonly IVolumeEnvelope _volumeEnvelope = new VolumeEnvelope();
    private int _timer;
    private int _clockShift;
    private bool _widthMode;
    private int _divisorCode;
    private int _lfsr;

    private byte _duty;

    public Channel4() 
    {
        _timer = 0;
        _clockShift = 0;
        _widthMode = false;
        _divisorCode = 0;
        _lfsr = 0x7FFF;

        LengthCounter.SetFullLength(64);
    }

    private void Trigger()
    {
        _volumeEnvelope.Trigger();

        _timer = divisors[_divisorCode] << _clockShift;

        _lfsr = 0x7fff;

        ChannelEnabled = DacEnabled;
    }
    
    public override void Tick()
    {
        if (--_timer <= 0) {
            _timer = divisors[_divisorCode] << _clockShift;

            var result = ((_lfsr & 1) ^ ((_lfsr >> 1) & 1)) != 0;
            _lfsr >>= 1;
            _lfsr |= result ? (1 << 14) : 0;

            if (_widthMode) {
                _lfsr &= ~0x40;
                _lfsr |= result ? 0x40 : 0;
            }

            if (IsEnabled() && (_lfsr & 1) == 0)
                Output = _volumeEnvelope.GetVolume();
            else
                Output = 0;
        }
    }

    public override void PowerOff()
    {
        _volumeEnvelope.PowerOff();
        LengthCounter.PowerOff(false);

        ChannelEnabled = false;
        DacEnabled = false;

        _clockShift  = 0;
        _widthMode   = false;
        _divisorCode = 0;
    }

    public override byte Read(ushort address)
    {
        switch (address) {
            case 0xFF1F:
                return 0xFF;

            case 0xFF20:
                return 0xFF;

            case 0xFF21:
                return _volumeEnvelope.GetNr2();

            case 0xFF22:
                return (byte)((_clockShift << 4) | (_widthMode ? 0x08 : 0) | _divisorCode);

            case 0xFF23:
                return (byte)((LengthCounter.IsEnabled() ? 0x40 : 0) | 0xBF);
        }

        // Can't Access provided address
        return 0;
    }


    public override void EnvelopeClock()
    {
        _volumeEnvelope.Step();
    }

    public override void Write(ushort address, byte value)
    {
        switch (address) {
            case 0xFF1F:
                return;

            case 0xFF20:
                LengthCounter.SetLength((byte)(value & 0x3F));
                return;

            case 0xFF21:
                DacEnabled = (value & 0xF8) != 0;
                ChannelEnabled &= DacEnabled;

                _volumeEnvelope.SetNr2(value);
                return;

            case 0xFF22:
                _clockShift = value >> 4;
                _widthMode = (value & 0x08) != 0;
                _divisorCode = value & 0b111;
                return;

            case 0xFF23:
                LengthCounter.SetNr4(value);

                if (LengthCounter.IsEnabled() && LengthCounter.IsZero())
                    ChannelEnabled = false;
                else if ((value & 0x80) > 0)
                    Trigger();
                return;
        }
    }
}