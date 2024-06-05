using System.Threading.Channels;
using GBEmulator.Core.Interfaces;
using GBEmulator.Core.Models;

namespace GBEmulator.Hardware.Components.Apu;

public class Channel1 : Channel
{
    private readonly bool[,] _dutyTable = new bool[4,8] {
        {false, false, false, false, false, false, false, true},
        {true, false, false, false, false, false, false, true},
        {true, false, false, false, false, true, true, true},
        {false, true, true, true, true, true, true, false}
    };
    
    private readonly IVolumeEnvelope _volumeEnvelope = new VolumeEnvelope();
    private readonly IFrequencySweep _frequencySweep = new FrequencySweep();
    private int _timer;
    private int _sequence;
    private byte _duty;

    public Channel1() 
    {
        _timer = 0;
        _sequence = 0;
        _duty = 0;

        LengthCounter.SetFullLength(64);
    }

    private void Trigger()
    {
        _timer = (2048 - _frequencySweep.GetFrequency()) << 2;

        _volumeEnvelope.Trigger();
        _frequencySweep.Trigger();

        if (_frequencySweep.IsEnabled())
            ChannelEnabled = DacEnabled;
        else
            ChannelEnabled = false;
    }
    
    public override void Tick()
    {
        if (--_timer <= 0) {
            _timer = (2048 - _frequencySweep.GetFrequency()) << 2;

            _sequence = (_sequence + 1) & 7;

            if (IsEnabled())
                Output = (byte)(_dutyTable[_duty, _sequence] ? _volumeEnvelope.GetVolume() : 0);
            else
                Output = 0;
        }
    }

    public override void SweepClock()
    {
        _frequencySweep.Step();

        if (!_frequencySweep.IsEnabled())
            ChannelEnabled = false;
    }

    public override void EnvelopeClock()
    {
        _volumeEnvelope.Step();
    }

    public override void PowerOff()
    {
        _frequencySweep.PowerOff();
        _volumeEnvelope.PowerOff();
        LengthCounter.PowerOff(false);

        ChannelEnabled = false;
        DacEnabled = false;

        _sequence = 0;
        _duty = 0;
    }

    public override byte Read(ushort address)
    {
        switch (address) {
            case 0xFF10:
                return (byte)(_frequencySweep.GetNr10() | 0x80);

            case 0xFF11:
                return (byte)((_duty << 6) | 0x3F);

            case 0xFF12:
                return _volumeEnvelope.GetNr2();

            case 0xFF13:
                return 0xFF;

            case 0xFF14:
                return (byte)((LengthCounter.IsEnabled() ? 0x40 : 0) | 0xBF);
        }

        // Can't access address
        return 0;
    }

    public override void Write(ushort address, byte value)
    {
        switch (address) {
            case 0xFF10:
                _frequencySweep.SetNr10(value);

                if (!_frequencySweep.IsEnabled())
                    ChannelEnabled = false;
                return;

            case 0xFF11:
                _duty = (byte)(value >> 6);
                LengthCounter.SetLength((byte)(value & 0x3F));
                return;

            case 0xFF12:
                DacEnabled = (value & 0xF8) != 0;
                ChannelEnabled &= DacEnabled;

                _volumeEnvelope.SetNr2(value);
                return;

            case 0xFF13:
                _frequencySweep.SetNr13(value);
                return;

            case 0xFF14:
                _frequencySweep.SetNr14(value);
                LengthCounter.SetNr4(value);

                if (LengthCounter.IsEnabled() && LengthCounter.IsZero())
                    ChannelEnabled = false;
                else if ((value & 0x80) > 0)
                    Trigger();
                return;
        }
    }
}