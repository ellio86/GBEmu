using GBEmulator.Core.Interfaces;

namespace GBEmulator.Hardware.Components.Apu;

public class FrequencySweep : IFrequencySweep
{
    private bool _enabled;
    private bool _overflow;
    private bool _hasNegated;
    private int _timer;
    private int _frequency;
    private int _shadowFrequency;
    private byte _period;
    private bool _negate;
    private byte _shift;

    public FrequencySweep()
    {
        PowerOff();
    }
    
    private int Calculate()
    {
        var newFrequency = _shadowFrequency >> _shift;

        if (_negate) {
            newFrequency = _shadowFrequency - newFrequency;
            _hasNegated = true;
        }
        else
            newFrequency = _shadowFrequency + newFrequency;

        if (newFrequency > 2047)
            _overflow = true;

        return newFrequency;
    }
    
    public void Step()
    {
        if (_enabled)
            return;

        if (--_timer <= 0) {
            _timer = (_period != 0) ? _period : 8;

            if (_period != 0) {
                var newFrequency = Calculate();

                if (!_overflow && _shift != 0) {
                    _shadowFrequency = newFrequency;
                    _frequency = newFrequency;

                    Calculate();
                }
            }
        }
    }

    public bool IsEnabled()
    {
        return _overflow is false;
    }

    public void PowerOff()
    {
        _enabled = false;
        _overflow = false;
        _hasNegated = false;

        _timer = 0;

        _frequency = 0;
        _shadowFrequency = 0;

        _period = 0;
        _negate = false;
        _shift  = 0;
    }

    public void SetNr10(byte value)
    {
        _period = (byte)((value >> 4) & 0b111);
        _negate = (value & 0x08) != 0;
        _shift  = (byte)(value & 0b111);
        
        if (_hasNegated && !_negate)
        {
            _overflow = true;
        }
    }

    public void SetNr13(byte value)
    {
        _frequency = (_frequency & 0x700) | value;
    }

    public void SetNr14(byte value)
    {
        _frequency = (_frequency & 0xFF) | ((value & 0b111) << 8);
    }

    public byte GetNr10()
    {
        return (byte)((_period << 4) | (_negate ? 0x08 : 0) | _shift);
    }

    public int GetFrequency()
    {
        return _frequency;
    }

    public void Trigger()
    {
        _overflow = false;
        _hasNegated = false;

        _shadowFrequency = _frequency;

        _timer  = (_period != 0) ? _period : 8;
        _enabled = _period != 0 || _shift != 0;

        if (_shift > 0)
            Calculate();
    }
}