namespace GBEmulator.App;

using Core.Enums;
using Core.Interfaces;

public class Registers : IRegisters
{
    public byte A { get; set; } = 0x00;
    public byte B { get; set; } = 0x00;
    public byte C { get; set; } = 0x00;
    public byte D { get; set; } = 0x00;
    public byte E { get; set; } = 0x00;
    public byte F { get; set; } = 0x00;
    public byte H { get; set; } = 0x00;
    public byte L { get; set; } = 0x00;
    public ushort AF { get => GetAF(); set => SetAF(value); }
    public ushort BC { get => GetBC(); set => SetBC(value); }
    public ushort DE { get => GetDE(); set => SetDE(value); }
    public ushort HL { get => GetHL(); set => SetHL(value); }
    public ushort SP { get; set; } = 0x0000;
    public ushort PC { get; set; } = 0x0000;

    #region Virtual 16 bit register getters/setters

    public ushort GetAF()
    {
        return (ushort)((A << 8) | F);
    }

    public void SetAF(ushort value)
    {
        A = (byte)((value & 0xFF00) >> 8);
        F = (byte)(value & 0x00FF);
    }

    public ushort GetBC()
    {
        return (ushort)((B << 8) | C);
    }

    public void SetBC(ushort value)
    {
        B = (byte)((value & 0xFF00) >> 8);
        C = (byte)(value & 0x00FF);
    }

    public ushort GetDE()
    {
        return (ushort)((D << 8) | E);
    }

    public void SetDE(ushort value)
    {
        D = (byte)((value & 0xFF00) >> 8);
        E = (byte)(value & 0x00FF);
    }


    public ushort GetHL()
    {
        return (ushort)((H << 8) | L);
    }

    public void SetHL(ushort value)
    {
        H = (byte)((value & 0xFF00) >> 8);
        L = (byte)(value & 0x00FF);
    }

    #endregion

    public bool GetFlag(Flag flag)
    {
        return (F & (byte)flag) > 0;
    }

    public void SetFlag(Flag flag, bool value)
    {
        if (value)
        {
            F |= (byte)flag;
        }
        else
        {
            F &= ((byte)~flag);
        }
    }
}
