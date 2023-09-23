namespace GBEmulator.Hardware;

using Core.Enums;
using Core.Interfaces;

public class Registers : IRegisters
{
    public byte A { get; set; } 
    public byte B { get; set; } 
    public byte C { get; set; } 
    public byte D { get; set; } 
    public byte E { get; set; } 
    public byte F { get; set; } 
    public byte H { get; set; } 
    public byte L { get; set; } 
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

    public void SetCarryFlags(byte value1, byte value2)
    {
        SetFlag(Flag.Carry, (ushort)(value1 + value2) > 0xFF);
        SetHalfCarryFlag(value1, value2);
    }

    public void SetCarryFlags(ushort value1, sbyte value2)
    {
        SetFlag(Flag.Carry, (ushort)(value1 + value2) > 0xFF);
        if (value2 < 0)
        {
            SetHalfCarryFlagSubtracting(value1, (byte)(value2 * -1));
            return;
        }
        SetHalfCarryFlag(value1, (byte)value2);
    }

    public void SetCarryFlags(ushort value1, ushort value2)
    {
        SetFlag(Flag.Carry, (value1 + value2) > 0xFFFF);
        SetHalfCarryFlag(value1, value2);
    }

    public void SetHalfCarryFlag(byte value1, byte value2)
    {
        SetFlag(Flag.HalfCarry, (byte)((value1 & 0xF) + (value2 & 0xF)) > 0xF);
    }
    public void SetHalfCarryFlagSubtracting(byte value1, byte value2)
    {
        SetFlag(Flag.HalfCarry, ((((value1 & 0xf) - (value2 & 0xf)) & 0x10) != 0));
    }

    public void SetHalfCarryFlagSubtracting(ushort value1, ushort value2)
    {
        SetFlag(Flag.HalfCarry, ((((value1 & 0xff) - (value2 & 0xff)) & 0x100) != 0));
    }

    public void SetHalfCarryFlag(ushort value1, ushort value2)
    {
        SetFlag(Flag.HalfCarry, ((value1 & 0x0FFF) + (value2 & 0x0FFF)) > 0x0FFF);
    }
}
