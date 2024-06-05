using GBEmulator.Core.Interfaces;

namespace GBEmulator.Hardware.Components.Apu;

public class VolumeEnvelope : IVolumeEnvelope
{
    private bool _finished;
    private int _timer;

    private byte _startingVolume;
    private bool _addMode;
    private byte _period;

    private byte _volume;
    
    public VolumeEnvelope()
    {
        PowerOff();
    }
    
    public void Step()
    {
        if (_finished)
            return;

        if (--_timer <= 0) {
            _timer = (_period != 0) ? _period : 8;

            if (_addMode && _volume < 15)
                _volume++;
            else if (!_addMode && _volume > 0)
                _volume--;

            if (_volume == 0 || _volume == 15)
                _finished = true;
        }
    }

    public void PowerOff()
    {
        _finished = true;
        _timer = 0;
        _startingVolume = 0;
        _addMode = false;
        _period = 0;
        _volume = 0;
    }

    public void SetNr2(byte value)
    {
        _startingVolume = (byte)(value >> 4);
        _addMode = (value & 0x08) != 0;
        _period = (byte)(value & 0b111);
    }

    public byte GetNr2()
    {
        return (byte)((_startingVolume << 4) | (_addMode ? 0x08 : 0) | _period);
    }

    public byte GetVolume()
    {
        if (_period > 0)
        {
            return _volume;
        }
        else
        {
            return _startingVolume;
        }
    }

    public void Trigger()
    {
        _volume = _startingVolume;

        _finished = false;
        _timer = (_period != 0) ? _period : 8;
    }
}