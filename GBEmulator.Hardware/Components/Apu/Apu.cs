namespace GBEmulator.Hardware.Components.Apu;

using Core.Interfaces;
using Core.Models;

public class Apu : HardwareComponent, IApu
{
    private IAudioDriver _audioDriver;
    private IChannel[] Channels { get; } = new IChannel[4];
    private float[] Volume { get; } = new float[4];
    private bool[] LeftEnabled { get; } = new bool[4];
    private bool[] RightEnabled { get; } = new bool[4];

    private bool VinLeftEnable { get; set; }
    private bool VinRightEnable { get; set; }
    private byte LeftVolume { get; set; }
    private byte RightVolume { get; set; }
    private bool Enabled { get; set; }
    private int FrequencyCounter { get; set; }
    private int FrameSequencerCounter { get; set; }
    private int FrameSequencer { get; set; }

    public override void ConnectToBus(IBus bus)
    {
        base.ConnectToBus(bus);
        foreach (var channel in Channels)
        {
            channel?.ConnectToBus(bus);
        }
    }

    public Apu()
    {
        Channels[0] = new Channel1();
        Channels[1] = new Channel2(); 
        Channels[2] = new Channel3();
        Channels[3] = new Channel4();

        VinLeftEnable = VinRightEnable = false;
        LeftVolume = RightVolume = 0;
        Enabled = false;

        for (var i = 0; i < 4; i++)
        {
            LeftEnabled[i] = false;
            RightEnabled[i] = false;
            Volume[i] = 1.0f;
        }

        FrequencyCounter = 95;

        FrameSequencerCounter = 8192;
        FrameSequencer = 0;
    }

    public void BindAudioDriver(IAudioDriver audioDriver)
    {
        _audioDriver = audioDriver;
    }

    public void Tick()
    {
        if (--FrameSequencerCounter <= 0)
        {
            FrameSequencerCounter = 8192;

            switch (FrameSequencer)
            {
                case 0:
                    Channels[0].LengthClock();
                    Channels[1].LengthClock();
                    Channels[2].LengthClock();
                    Channels[3].LengthClock();
                    break;
                case 1:
                    break;

                case 2:
                    Channels[0].SweepClock();
                    Channels[0].LengthClock();
                    Channels[1].LengthClock();
                    Channels[2].LengthClock();
                    Channels[3].LengthClock();
                    break;

                case 3:
                    break;

                case 4:
                    Channels[0].LengthClock();
                    Channels[1].LengthClock();
                    Channels[2].LengthClock();
                    Channels[3].LengthClock();
                    break;

                case 5:
                    break;
                case 6:
                    Channels[0].SweepClock();
                    Channels[0].LengthClock();
                    Channels[1].LengthClock();
                    Channels[2].LengthClock();
                    Channels[3].LengthClock();
                    break;
                case 7:
                    Channels[0].EnvelopeClock();
                    Channels[1].EnvelopeClock();
                    Channels[3].EnvelopeClock();
                    break;
            }

            FrameSequencer = (FrameSequencer + 1) & 0b0111;

            Channels[0].SetFrameSequencer(FrameSequencer);
            Channels[1].SetFrameSequencer(FrameSequencer);
            Channels[2].SetFrameSequencer(FrameSequencer);
            Channels[3].SetFrameSequencer(FrameSequencer);
        }

        // Tick Channels
        Channels[0].Tick();
        Channels[1].Tick();
        Channels[2].Tick();
        Channels[3].Tick();

        if (--FrequencyCounter <= 0)
        {
            FrequencyCounter = 95;

            int left = 0, right = 0;

            for (var i = 0; i < 4; i++)
            {
                if (Channels[i] is null) continue;
                var output = (byte)(Channels[i].GetOutput() * Volume[i]);

                if (LeftEnabled[i])
                    left += output;
                if (RightEnabled[i])
                    right += output;
            }

            if (_audioDriver is not null)
                _audioDriver.AddSample(ConvertSample(left), ConvertSample(right));
        }
    }

    public byte Read(ushort address)
    {
        if (address >= 0xFF10 && address <= 0xFF14)
            return Channels[0].Read(address);

        else if (address >= 0xFF15 && address <= 0xFF19)
            return Channels[1].Read(address);

        else if (address >= 0xFF1A && address <= 0xFF1E)
            return 0;
        //return Channels[2].Read(address);

        else if (address >= 0xFF1F && address <= 0xFF23)
            return Channels[3].Read(address);

        else if (address >= 0xFF27 && address <= 0xFF2F)
            return 0xFF;

        else if (address >= 0xFF30 && address <= 0xFF3F)
            return 0;
        //return Channels[2].Read(address);

        byte result = 0;

        switch (address)
        {
            case 0xFF24:
                return (byte)((VinLeftEnable ? 0x80 : 0) | (LeftVolume << 4) | (VinRightEnable ? 0x08 : 0) |
                              RightVolume);

            case 0xFF25:
                for (var i = 0; i < 4; i++)
                {
                    result |= RightEnabled[i] ? (byte)(1 << i) : (byte)0;
                    result |= LeftEnabled[i] ? (byte)(16 << i) : (byte)0;
                }

                return result;

            case 0xFF26:
                result = Enabled ? (byte)0x80 : (byte)0;

                for (var i = 0; i < 4; i++)
                    result |= Channels[i].IsEnabled() ? (byte)(1 << i) : (byte)0;

                return (byte)(result | 0x70);
        }

        return 0x0;
    }

    public void Write(ushort address, byte value)
    {
        if (address == 0xFF26)
        {
            var enable = (value & 0x80) != 0;

            // Clear registers when powered off
            if (Enabled && !enable)
                ClearRegisters();
            // Reset Frame-Sequencer when powered on
            else if (!Enabled & enable)
                FrameSequencer = 0;

            Enabled = enable;
            return;
        }

        else if (address >= 0xFF30 && address <= 0xFF3F)
        {
            //Channels[2].Write(address, value);
            return;
        }

        if (!Enabled)
        {
            // Power off does not affect length-counter writes
            switch (address)
            {
                case 0xFF11:
                    Channels[0].Write(address, (byte)(value & 0x3F));
                    return;

                case 0xFF16:
                    Channels[1].Write(address, (byte)(value & 0x3F));
                    return;

                case 0xFF1B:
                    //Channels[2].Write(address, value);
                    return;

                case 0xFF20:
                    Channels[3].Write(address, (byte)(value & 0x3F));
                    return;
            }

            return;
        }

        if (address >= 0xFF10 && address <= 0xFF14)
        {
            Channels[0].Write(address, value);
            return;
        }

        else if (address >= 0xFF15 && address <= 0xFF19)
        {
            Channels[1].Write(address, value);
            return;
        }

        else if (address >= 0xFF1A && address <= 0xFF1E)
        {
            Channels[2].Write(address, value);
            return;
        }

        else if (address >= 0xFF1F && address <= 0xFF23)
        {
            Channels[3].Write(address, value);
            return;
        }

        if (address >= 0xFF27 && address <= 0xFF2F)
            return;

        switch (address)
        {
            case 0xFF24:
                RightVolume = (byte)(value & 0b111);
                VinRightEnable = (value & 0x08) != 0;

                LeftVolume = (byte)((value >> 4) & 0b111);
                VinLeftEnable = (value & 0x80) != 0;
                return;

            case 0xFF25:
                for (var i = 0; i < 4; i++)
                {
                    RightEnabled[i] = ((value >> i) & 1) != 0;
                    LeftEnabled[i] = ((value >> (i + 4)) & 1) != 0;
                }

                return;
        }
    }

    private void ClearRegisters()
    {
        VinLeftEnable = VinRightEnable = false;
        LeftVolume = RightVolume = 0;

        Enabled = false;

        foreach (var channel in Channels)
        {
            channel?.PowerOff();
        }

        for (var i = 0; i < 4; i++) {
            LeftEnabled [i] = false;
            RightEnabled[i] = false;
        }
    }

    private int ConvertSample(int sample)
    {
        return (sample - 32) << 10;
    }
}