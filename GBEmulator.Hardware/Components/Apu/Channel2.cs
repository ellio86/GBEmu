using System.Threading.Channels;
using GBEmulator.Core.Interfaces;
using GBEmulator.Core.Models;

namespace GBEmulator.Hardware.Components.Apu;

public class Channel2 : HardwareComponent, IChannel
{
    private bool[,] _dutyTable = new bool[4,8] {
        {false, false, false, false, false, false, false, true},
        {true, false, false, false, false, false, false, true},
        {true, false, false, false, false, true, true, true},
        {false, true, true, true, true, true, true, false}
    };
    
    private IVolumeEnvelope _volumeEnvelope;
    private int _timer;
    private int _sequence;
    private int _frequency;

    private byte _duty;

    public Channel2(IBus bus)
    {
        ConnectToBus(bus);
        _timer = 0;
        _sequence = 0;
        _frequency = 0;
        _duty = 0;

        LengthCounter.SetFullLength(64);
    }

    private void Trigger()
    {
        _timer = (2048 - _frequency) << 2;

        _volumeEnvelope.Trigger();

        ChannelEnabled = DacEnabled;
    }
    
    public void Tick()
    {
        if (--_timer <= 0) {
            _timer = (2048 - _frequency) << 2;

            _sequence = (_sequence + 1) & 7;

            if (IsEnabled())
                Output = _dutyTable[_duty, _sequence] ? _volumeEnvelope.GetVolume() : (byte)0b0;
            else
                Output = 0;
        }
    }

    public void PowerOff()
    {
        _volumeEnvelope.PowerOff();
        LengthCounter.PowerOff(false);

        ChannelEnabled = false;
        DacEnabled = false;

        _sequence = 0;
        _frequency = 0;
        _duty = 0;
    }

    public byte GetOutput()
    {
        return Output;
    }

    public byte Read(ushort address)
    {
        switch (address)
        {
            case 0xFF15:
                return 0xFF;

            case 0xFF16:
                return (byte)((_duty << 6) | 0x3F);

            case 0xFF17:
                return _volumeEnvelope.GetNr2();

            case 0xFF18:
                return 0xFF;

            case 0xFF19:
                return (byte)((LengthCounter.IsEnabled() ? 0x40 : 0) | 0xBF);
        }

        // Cant access provided address
        return 0;
    }

    public void Write(ushort address, byte value)
    {
        switch (address) {
            case 0xFF15:
                return;

            case 0xFF16:
                _duty = (byte)(value >> 6);
                LengthCounter.SetLength((byte)(value & 0x3F));
                return;

            case 0xFF17:
                DacEnabled = (value & 0xF8) != 0;
                ChannelEnabled &= DacEnabled;

                _volumeEnvelope.SetNr2(value);
                return;

            case 0xFF18:
                _frequency = (_frequency & 0x700) | value;
                return;

            case 0xFF19:
                _frequency = (_frequency & 0xFF) | ((value & 0b111) << 8);
                LengthCounter.SetNr4(value);

                if (LengthCounter.IsEnabled() && LengthCounter.IsZero())
                    ChannelEnabled = false;
                else if ((value & 0x80) > 0)
                    Trigger();
                return;
        }
    }

    public bool IsEnabled()
    {
        return ChannelEnabled && DacEnabled;
    }

    public void LengthClock()
    {
        LengthCounter.Step();

        if (LengthCounter.IsEnabled() && LengthCounter.IsZero())
            ChannelEnabled = false;
    }

    public void SweepClock()
    {
    }

    public void EnvelopeClock()
    {
    }

    public void SetFrameSequencer(int frameSequencer)
    {
        LengthCounter.SetFrameSequencer(frameSequencer);
    }

    public ILengthCounter LengthCounter { get; set; } = new LengthCounter();
    public bool ChannelEnabled { get; set; }
    public bool DacEnabled { get; set; }
    public byte Output { get; set; }
}