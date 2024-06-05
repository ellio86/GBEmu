using GBEmulator.Core.Interfaces;

namespace GBEmulator.Hardware.Components.Apu;

public class LengthCounter: ILengthCounter
{
    private bool _enabled { get; set; }
    private int _length { get; set; }
    private int _fullLength { get; set; }
    private int _frameSequencer { get; set; }
    
    public LengthCounter()
    {
        _enabled = false;
        _length = 0;

        _fullLength = 0;
        _frameSequencer = 0;
    }
    
    public void Step()
    {
        if (_enabled && _length > 0)
        {
            _length--;
        }
    }

    public void SetNr4(byte value)
    {
        var enable = (value & 0x40) != 0;
        var trigger = (value & 0x80) != 0;

        if (_enabled)
        {
            if (trigger && _length == 0)
            {
                if (enable && (_frameSequencer & 1) > 0)
                {
                    _length = _fullLength - 1;
                }
                else
                {
                    _length = _fullLength;
                }
            }
        }
        else if (enable)
        {
            if ((_frameSequencer & 1) > 0) {
                if (_length != 0)
                {
                    _length--; 
                }

                if (trigger && _length == 0)
                {
                    _length = _fullLength - 1; 
                }
            }
        }
        
        else {
            if (trigger && _length == 0)
                _length = _fullLength;
        }

        _enabled = enable;
    }

    public bool IsEnabled()
    {
        return _enabled;
    }

    public bool IsZero()
    {
        return _length == 0;
    }

    public void SetLength(byte length)
    {
        if (_length == 0)
        {
            _length = _fullLength;
        }
        else
        {
            _length = _fullLength - _length;
        }
    }

    public void SetFullLength(int fullLength)
    {
        _fullLength = fullLength;
    }

    public void PowerOff(bool gbcMode)
    {
        _enabled = false;
        _frameSequencer = 0;

        if (gbcMode)
        {
            _length = _fullLength;
        }
    }

    public void SetFrameSequencer(int frameSequencer)
    {
        _frameSequencer = frameSequencer;
    }
}