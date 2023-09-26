namespace GBEmulator.Hardware;

using Core.Enums;
using Core.Interfaces;

public class Registers : IRegisters
{
    public byte A { get; set; }     // Accumulator
    public byte B { get; set; }     // Register B
    public byte C { get; set; }     // Register C
    public byte D { get; set; }     // Register D
    public byte E { get; set; }     // Register E
    public byte F { get; set; }     // Flags Register
    public byte H { get; set; }     // Register H
    public byte L { get; set; }     // Register L
    public ushort AF { get => GetAF(); set => SetAF(value); }   // Virtual 16-bit register using registers A & F
    public ushort BC { get => GetBC(); set => SetBC(value); }   // Virtual 16-bit register using registers B & C
    public ushort DE { get => GetDE(); set => SetDE(value); }   // Virtual 16-bit register using registers D & E
    public ushort HL { get => GetHL(); set => SetHL(value); }   // Virtual 16-bit register using registers H & L
    public ushort SP { get; set; } = 0x0000;    // Stack Pointer
    public ushort PC { get; set; } = 0x0000;    // Program Counter

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

    public void SetCarryFlags8Bit(int value1, int value2)
    {
        var sum = value1 + value2;
        var noCarrySum = value1 ^ value2;
        var carryInto = sum ^ noCarrySum;
        var halfCarry = (carryInto & 0x10) > 0;
        var carry = (carryInto & 0x100 ) > 0;
        SetFlag(Flag.Carry, carry);
        SetFlag(Flag.HalfCarry, halfCarry);
    }

    public void SetCarryFlags16Bit(int value1, int value2)
    {
        var sum = value1 + value2;
        var noCarrySum = value1 ^ value2;
        var carryInto = sum ^ noCarrySum;
        var halfCarry = (carryInto & 0x100) > 0;
        var carry = (carryInto & 0x10000 ) > 0;
        SetFlag(Flag.Carry, carry);
        SetFlag(Flag.HalfCarry, halfCarry);
    }

    public void SetCarryFlags2shorts(ushort value1, ushort value2)
    {
        SetFlag(Flag.Carry, (value1 + value2) > 0xFFFF);
        SetHalfCarryFlag(value1, value2);
    }
    
    public void SetHalfCarryFlag(ushort value1, byte value2)
    {
        SetFlag(Flag.HalfCarry, (byte)((value1 & 0xF) + (value2 & 0xF)) > 0xF);
    }
    public void SetHalfCarryFlagSubtracting(ushort value1, byte value2)
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
