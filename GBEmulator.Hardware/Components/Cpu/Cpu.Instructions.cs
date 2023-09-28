﻿namespace GBEmulator.Hardware.Components.Cpu;

using Core.Enums;
using System;

public partial class Cpu
{
    private bool _haltBug = false;

    /// <summary>
    /// Load Instruction
    /// </summary>
    private void LD(InstructionParam loadTo, InstructionParam dataToLoad)
    {
        byte data;
        byte extraData = 0x00;
        ushort addressToRead;
        switch (dataToLoad)
        {
            case InstructionParam.A:
                data = Registers.A;
                break;
            case InstructionParam.B:
                data = Registers.B;
                break;
            case InstructionParam.C:
                data = Registers.C;
                break;
            case InstructionParam.D:
                data = Registers.D;
                break;
            case InstructionParam.E:
                data = Registers.E;
                break;
            case InstructionParam.L:
                data = Registers.L;
                break;
            case InstructionParam.H:
                data = Registers.H;
                break;
            case InstructionParam.HL:
                extraData = Registers.H;
                data = Registers.L;
                break;
            case InstructionParam.d8:
                data = _bus.ReadMemory(Registers.PC);
                Registers.PC++;
                _cyclesLeft--;
                break;
            case InstructionParam.d16:
                data = _bus.ReadMemory(Registers.PC);
                Registers.PC++;
                _cyclesLeft--;
                extraData = _bus.ReadMemory(Registers.PC);
                Registers.PC++;
                _cyclesLeft--;
                break;
            case InstructionParam.BCMem:
                data = _bus.ReadMemory(Registers.BC);
                _cyclesLeft--;
                break;
            case InstructionParam.DEMem:
                data = _bus.ReadMemory(Registers.DE);
                _cyclesLeft--;
                break;
            case InstructionParam.HLMem:
                data = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                break;
            case InstructionParam.HLIMem:
                data = _bus.ReadMemory(Registers.HL);
                Registers.HL++;
                _cyclesLeft--;
                break;
            case InstructionParam.HLDMem:
                data = _bus.ReadMemory(Registers.HL);
                Registers.HL--;
                _cyclesLeft--;
                break;
            case InstructionParam.CMem:
                addressToRead = (ushort)(0xFF00 + Registers.C);
                data = _bus.ReadMemory(addressToRead);
                _cyclesLeft--;
                break;
            case InstructionParam.a8Mem:
                addressToRead = (ushort)(0xFF00 + _bus.ReadMemory(Registers.PC));
                Registers.PC++;
                _cyclesLeft--;

                data = _bus.ReadMemory(addressToRead);
                _cyclesLeft--;
                break;
            case InstructionParam.SP:
                data = (byte)(Registers.SP & 0x00FF);
                extraData = (byte)((Registers.SP & 0xFF00) >> 8);
                break;
            case InstructionParam.a16Mem:
                var lowByte = _bus.ReadMemory(Registers.PC);
                Registers.PC++;
                _cyclesLeft--;
                var highByte = _bus.ReadMemory(Registers.PC);
                Registers.PC++;
                _cyclesLeft--;

                addressToRead = (ushort)((highByte << 8) + lowByte);

                data = _bus.ReadMemory(addressToRead);
                _cyclesLeft--;
                break;
            case InstructionParam.SPs8:
                Registers.SetFlag(Flag.Zero, false);
                Registers.SetFlag(Flag.Subtraction, false);
                Registers.SetCarryFlags8Bit((sbyte)_bus.ReadMemory(Registers.PC), Registers.SP);
                var calculatedVal = (sbyte)_bus.ReadMemory(Registers.PC) + Registers.SP;
                data = (byte)calculatedVal;
                extraData = (byte)((calculatedVal & 0xFF00) >> 8);
                Registers.PC++;
                _cyclesLeft -= 2;
                break;
            default:
                throw new InvalidOperationException(nameof(dataToLoad));
        }

        ushort addressToWrite;
        switch (loadTo)
        {
            case InstructionParam.A:
                Registers.A = data;
                break;
            case InstructionParam.B:
                Registers.B = data;
                break;
            case InstructionParam.C:
                Registers.C = data;
                break;
            case InstructionParam.D:
                Registers.D = data;
                break;
            case InstructionParam.E:
                Registers.E = data;
                break;
            case InstructionParam.L:
                Registers.L = data;
                break;
            case InstructionParam.H:
                Registers.H = data;
                break;
            case InstructionParam.BCMem:
                _bus.WriteMemory(Registers.BC, data);
                _cyclesLeft--;
                break;
            case InstructionParam.DEMem:
                _bus.WriteMemory(Registers.DE, data);
                _cyclesLeft--;
                break;
            case InstructionParam.HLMem:
                _bus.WriteMemory(Registers.HL, data);
                _cyclesLeft--;
                break;
            case InstructionParam.HLIMem:
                _bus.WriteMemory(Registers.HL, data);
                Registers.HL++;
                _cyclesLeft--;
                break;
            case InstructionParam.HLDMem:
                _bus.WriteMemory(Registers.HL, data);
                Registers.HL--;
                _cyclesLeft--;
                break;
            case InstructionParam.BC:
                Registers.C = data;
                Registers.B = extraData;
                break;
            case InstructionParam.DE:
                Registers.E = data;
                Registers.D = extraData;
                break;
            case InstructionParam.HL:
                Registers.L = data;
                Registers.H = extraData;
                break;
            case InstructionParam.SP:
                Registers.SP = (ushort)((extraData << 8) + data);
                break;
            case InstructionParam.CMem:
                addressToWrite = (ushort)(0xFF00 + Registers.C);
                _bus.WriteMemory(addressToWrite, data);
                _cyclesLeft--;
                break;
            case InstructionParam.a8Mem:
                addressToWrite = (ushort)(0xFF00 + _bus.ReadMemory(Registers.PC));
                _cyclesLeft--;
                Registers.PC++;

                _bus.WriteMemory(addressToWrite, data);
                _cyclesLeft--;
                break;
            case InstructionParam.a16Mem:
                var lowByte = _bus.ReadMemory(Registers.PC);
                Registers.PC++;
                _cyclesLeft--;
                var highByte = _bus.ReadMemory(Registers.PC);
                Registers.PC++;
                _cyclesLeft--;
                addressToWrite = (ushort)((highByte << 8) + lowByte);

                if (dataToLoad is InstructionParam.SP)
                {

                    _bus.WriteMemory(addressToWrite, data);
                    _cyclesLeft--;

                    _bus.WriteMemory((ushort)(addressToWrite + 1), extraData);
                    _cyclesLeft--;
                    break;
                }

                _bus.WriteMemory(addressToWrite, data);
                _cyclesLeft--;
                break;
            default:
                throw new InvalidOperationException(loadTo.ToString());
        }
    }

    /// <summary>
    /// Add the second param to the first one and store the result wherever the first param is stored
    /// </summary>
    /// <param name="paramToAddTo">Value being added to/updated</param>
    /// <param name="paramToAdd">Value to add</param>
    /// <exception cref="NotSupportedException"></exception>
    private void ADD(InstructionParam paramToAddTo, InstructionParam paramToAdd)
    {
        var valueToAdd = 0;
        switch (paramToAdd)
        {
            case InstructionParam.A:
                valueToAdd = Registers.A;
                break;
            case InstructionParam.B:
                valueToAdd = Registers.B;
                break;
            case InstructionParam.C:
                valueToAdd = Registers.C;
                break;
            case InstructionParam.D:
                valueToAdd = Registers.D;
                break;
            case InstructionParam.E:
                valueToAdd = Registers.E;
                break;
            case InstructionParam.H:
                valueToAdd = Registers.H;
                break;
            case InstructionParam.L:
                valueToAdd = Registers.L;
                break;
            case InstructionParam.BC:
                valueToAdd = Registers.BC;
                break;
            case InstructionParam.DE:
                valueToAdd = Registers.DE;
                break;
            case InstructionParam.HL:
                valueToAdd = Registers.HL;
                break;
            case InstructionParam.SP:
                valueToAdd = Registers.SP;
                break;
            case InstructionParam.HLMem:
                valueToAdd = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                valueToAdd = _bus.ReadMemory(Registers.PC);
                _cyclesLeft--;
                Registers.PC++;
                break;
            case InstructionParam.s8:
                valueToAdd = (sbyte)_bus.ReadMemory(Registers.PC);
                _cyclesLeft--;
                Registers.PC++;
                break;
            default:
                throw new NotSupportedException(paramToAdd.ToString());
        }

        switch (paramToAddTo)
        {
            case InstructionParam.A:
                Registers.SetCarryFlags8Bit(Registers.A, valueToAdd);
                Registers.A = (byte)(valueToAdd + Registers.A);
                Registers.SetFlag(Flag.Zero, Registers.A == 0x00);
                break;
            case InstructionParam.HL:
                Registers.SetCarryFlags2shorts(Registers.HL, (ushort)valueToAdd);
                Registers.HL += (ushort)valueToAdd;
                _cyclesLeft--;
                break;
            case InstructionParam.SP:
                Registers.SetCarryFlags8Bit(Registers.SP, valueToAdd);
                Registers.SetFlag(Flag.Zero, false);
                Registers.SP = (ushort)(Registers.SP + (sbyte)valueToAdd);
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(paramToAdd.ToString());
        }
        Registers.SetFlag(Flag.Subtraction, false);
    }

    /// <summary>
    /// Add the second param to the first one and store the result wherever the first param is stored
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    private void SUB(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.A:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.A & 0xf)) & 0x10) != 0 );
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.A & 0xff)) & 0x100) != 0 );
                Registers.A = (byte)(Registers.A - Registers.A);
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.B:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.B & 0xf)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.B & 0xff)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.B);
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.C:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.C & 0xf)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.C & 0xff)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.C);
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.D:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.D & 0xf)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.D & 0xff)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.D);
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.E:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.E & 0xf)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.E & 0xff)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.E);
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.H:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.H & 0xf)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.H & 0xff)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.H);
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.L:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.L & 0xf)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.L & 0xff)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.L);
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.HLMem:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (_bus.ReadMemory(Registers.HL) & 0xf)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (_bus.ReadMemory(Registers.HL))) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - _bus.ReadMemory(Registers.HL));
                _cyclesLeft--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.d8:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (_bus.ReadMemory(Registers.PC) & 0xf)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (_bus.ReadMemory(Registers.PC))) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - _bus.ReadMemory(Registers.PC));
                Registers.PC++;
                _cyclesLeft--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            default:
                throw new ArgumentNullException(param1.ToString());
        }
    }

    /// <summary>
    /// Add the second param to the first one and store the result wherever the first param is stored
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    private void SBC(InstructionParam param1, InstructionParam param2)
    {
        switch (param2)
        {
            case InstructionParam.A:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.A & 0xf) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.A & 0xff) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.A - (Registers.GetFlag(Flag.Carry) ? 1 : 0));
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.B:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.B & 0xf) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.B & 0xff) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.B - (Registers.GetFlag(Flag.Carry) ? 1 : 0));
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.C:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.C & 0xf) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.C & 0xff) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.C - (Registers.GetFlag(Flag.Carry) ? 1 : 0));
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.D:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.D & 0xf) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.D & 0xff) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.D - (Registers.GetFlag(Flag.Carry) ? 1 : 0));
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.E:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.E & 0xf) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.E & 0xff) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.E - (Registers.GetFlag(Flag.Carry) ? 1 : 0));
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.H:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.H & 0xf) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.H & 0xff) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.H - (Registers.GetFlag(Flag.Carry) ? 1 : 0));
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.L:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (Registers.L & 0xf) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (Registers.L & 0xff) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - Registers.L - (Registers.GetFlag(Flag.Carry) ? 1 : 0));
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.HLMem:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (_bus.ReadMemory(Registers.HL) & 0xf) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (_bus.ReadMemory(Registers.HL)) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - _bus.ReadMemory(Registers.HL) - (Registers.GetFlag(Flag.Carry) ? 1 : 0));
                _cyclesLeft--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.d8:
                Registers.SetFlag(Flag.HalfCarry, (((Registers.A & 0xf) - (_bus.ReadMemory(Registers.PC) & 0xf) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                Registers.SetFlag(Flag.Carry, (((Registers.A & 0xff) - (_bus.ReadMemory(Registers.PC)) - (Registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                Registers.A = (byte)(Registers.A - _bus.ReadMemory(Registers.PC) - (Registers.GetFlag(Flag.Carry) ? 1 : 0));
                Registers.PC++;
                _cyclesLeft--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            default:
                throw new ArgumentNullException(param2.ToString());
        }

        switch (param1)
        {
            case InstructionParam.A:
                break;
            default:
                throw new InvalidOperationException(param1.ToString());
        }
    }

    /// <summary>
    /// Add paramToAdd + Carry bit to paramToAddTo
    /// </summary>
    /// <param name="paramToAddTo"></param>
    /// <param name="paramToAdd"></param>
    /// <exception cref="NotSupportedException"></exception>
    private void ADC(InstructionParam paramToAddTo, InstructionParam paramToAdd)
    {
        var carryValue = Registers.GetFlag(Flag.Carry) ? 1 : 0;
        byte valueToAdd;
        switch (paramToAdd)
        {
            case InstructionParam.A:
                valueToAdd = Registers.A;
                break;          
            case InstructionParam.B:
                valueToAdd = Registers.B;
                break;
            case InstructionParam.C:
                valueToAdd = Registers.C;
                break;
            case InstructionParam.D:
                valueToAdd = Registers.D;
                break;
            case InstructionParam.E:
                valueToAdd = Registers.E;
                break;
            case InstructionParam.H:
                valueToAdd = Registers.H;
                break;
            case InstructionParam.L:
                valueToAdd = Registers.B;
                break;
            case InstructionParam.HLMem:
                valueToAdd = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                valueToAdd = _bus.ReadMemory(Registers.PC);
                Registers.PC++;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(paramToAdd.ToString());
        }

        switch (paramToAddTo)
        {
            case InstructionParam.A:
                var carryOverride = false;
                if (Registers.GetFlag(Flag.Carry))
                {
                    Registers.A++;
                    if (Registers.A == 0)
                    {
                        carryOverride = true;
                    }
                }
                Registers.SetCarryFlags8Bit(Registers.A, valueToAdd);
                
                if(carryOverride) Registers.SetFlag(Flag.Carry, true);
                
                Registers.A += valueToAdd;
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                Registers.SetFlag(Flag.Subtraction, false);
                break;
            default:
                throw new NotSupportedException(paramToAdd.ToString());
        }
    }

    /// <summary>
    /// Increment the provided param by 1
    /// </summary>
    /// <param name="param"></param>
    /// <exception cref="NotSupportedException"></exception>
    private void INC(InstructionParam param)
    {
        switch (param)
        {
            case InstructionParam.A:
                Registers.SetHalfCarryFlag(Registers.A, 1);
                Registers.A++;
                Registers.SetFlag(Flag.Subtraction, false);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.B:
                Registers.SetHalfCarryFlag(Registers.B, 1);
                Registers.B++;
                Registers.SetFlag(Flag.Subtraction, false);
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                break;
            case InstructionParam.C:
                Registers.SetHalfCarryFlag(Registers.C, 1);
                Registers.C++;
                Registers.SetFlag(Flag.Subtraction, false);
                Registers.SetFlag(Flag.Zero, Registers.C == 0);
                break;
            case InstructionParam.D:
                Registers.SetHalfCarryFlag(Registers.D, 1);
                Registers.D++;
                Registers.SetFlag(Flag.Subtraction, false);
                Registers.SetFlag(Flag.Zero, Registers.D == 0);
                break;
            case InstructionParam.E:
                Registers.SetHalfCarryFlag(Registers.E, 1);
                Registers.E++;
                Registers.SetFlag(Flag.Subtraction, false);
                Registers.SetFlag(Flag.Zero, Registers.E == 0);
                break;
            case InstructionParam.H:
                Registers.SetHalfCarryFlag(Registers.H, 1);
                Registers.H++;
                Registers.SetFlag(Flag.Subtraction, false);
                Registers.SetFlag(Flag.Zero, Registers.H == 0);
                break;
            case InstructionParam.L:
                Registers.SetHalfCarryFlag(Registers.L, 1);
                Registers.L++;
                Registers.SetFlag(Flag.Subtraction, false);
                Registers.SetFlag(Flag.Zero, Registers.L == 0);
                break;
            case InstructionParam.BC:
                Registers.BC++;
                _cyclesLeft--;
                break;
            case InstructionParam.DE:
                Registers.DE++;
                _cyclesLeft--;
                break;
            case InstructionParam.HL:
                Registers.HL++;
                _cyclesLeft--;
                break;
            case InstructionParam.SP:
                Registers.SP++;
                _cyclesLeft--;
                break;
            case InstructionParam.HLMem:
                var valueToAddTo = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                Registers.SetHalfCarryFlag(valueToAddTo, 1);
                valueToAddTo++;
                _bus.WriteMemory(Registers.HL, valueToAddTo);
                _cyclesLeft--;
                Registers.SetFlag(Flag.Subtraction, false);
                Registers.SetFlag(Flag.Zero, valueToAddTo == 0);
                break;
            default:
                throw new NotSupportedException(param.ToString());
        }
    }

    /// <summary>
    /// Decrement the provided param by 1
    /// </summary>
    /// <param name="param"></param>
    /// <exception cref="NotSupportedException"></exception>
    private void DEC(InstructionParam param)
    {
        switch (param)
        {
            case InstructionParam.A:
                Registers.SetHalfCarryFlagSubtracting(Registers.A, 1);
                Registers.A--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.B:
                Registers.SetHalfCarryFlagSubtracting(Registers.B, 1);
                Registers.B--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                break;
            case InstructionParam.C:
                Registers.SetHalfCarryFlagSubtracting(Registers.C, 1);
                Registers.C--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.C == 0);
                break;
            case InstructionParam.D:
                Registers.SetHalfCarryFlagSubtracting(Registers.D, 1);
                Registers.D--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.D == 0);
                break;
            case InstructionParam.E:
                Registers.SetHalfCarryFlagSubtracting(Registers.E, 1);
                Registers.E--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.E == 0);
                break;
            case InstructionParam.H:
                Registers.SetHalfCarryFlagSubtracting(Registers.H, 1);
                Registers.H--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.H == 0);
                break;
            case InstructionParam.L:
                Registers.SetHalfCarryFlagSubtracting(Registers.L, 1);
                Registers.L--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, Registers.L == 0);
                break;
            case InstructionParam.BC:
                Registers.BC--;
                _cyclesLeft--;
                break;
            case InstructionParam.DE:
                Registers.DE--;
                _cyclesLeft--;
                break;
            case InstructionParam.HL:
                Registers.HL--;
                _cyclesLeft--;
                break;
            case InstructionParam.SP:
                Registers.SP--;
                _cyclesLeft--;
                break;
            case InstructionParam.HLMem:
                var valueToDecrement = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                Registers.SetHalfCarryFlagSubtracting(valueToDecrement, 1);
                valueToDecrement--;
                _bus.WriteMemory(Registers.HL, valueToDecrement);
                _cyclesLeft--;
                Registers.SetFlag(Flag.Subtraction, true);
                Registers.SetFlag(Flag.Zero, valueToDecrement == 0);
                break;
            default:
                throw new NotSupportedException(param.ToString());
        }
    }

    /// <summary>
    /// Loads param2 into the program counter if param1 condition is met
    /// </summary>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    private void JP(InstructionParam param1, InstructionParam param2)
    {
        var conditionMet = CheckCondition(param1);
        if (conditionMet is null)
        {
            switch (param1)
            {
                case InstructionParam.a16Mem:
                    var lowByte = _bus.ReadMemory(Registers.PC);
                    Registers.PC++;
                    _cyclesLeft--;

                    var highByte = _bus.ReadMemory(Registers.PC);
                    Registers.PC++;
                    _cyclesLeft--;

                    var address = (ushort)((highByte << 8) + lowByte);
                    Registers.PC = address;
                    _cyclesLeft--;
                    break;
                case InstructionParam.HL:
                    Registers.PC = Registers.HL;
                    break;
                default:
                    throw new NotSupportedException(param1.ToString());
            }

            return;
        }

        switch (param2)
        {
            case InstructionParam.a16Mem:
                if ((bool)conditionMet)
                {
                    var lowByte = _bus.ReadMemory(Registers.PC);
                    Registers.PC++;
                    _cyclesLeft--;

                    var highByte = _bus.ReadMemory(Registers.PC);
                    Registers.PC++;
                    _cyclesLeft--;

                    var address = (ushort)((highByte << 8) + lowByte);
                    Registers.PC = address;
                    _cyclesLeft--;
                }
                else
                {
                    _cyclesLeft -= 3;
                    Registers.PC += 2;
                }
                break;
            default:
                throw new NotSupportedException(param2.ToString());
        }
    }

    /// <summary>
    /// Jump Relative. Jump param2 (s8) steps from the current address in the PC if param1 condition is met
    /// </summary>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    private void JR(InstructionParam param1, InstructionParam param2)
    {
        void Jump(sbyte steps)
        {
            Registers.PC = (ushort)(steps + Registers.PC);
            _cyclesLeft--;
        }

        var conditionMet = CheckCondition(param1);
        if (conditionMet is null)
        {
            Jump((sbyte)_bus.ReadMemory(Registers.PC));
            Registers.PC++;
            _cyclesLeft--;
            return;
        }

        switch (param2)
        {
            case InstructionParam.s8:
                if ((bool)conditionMet)
                {
                    Jump((sbyte)_bus.ReadMemory(Registers.PC));
                    Registers.PC++;
                    _cyclesLeft--;
                }
                else
                {
                    _cyclesLeft -= 2;
                    Registers.PC++;
                }
                break;
            default:
                throw new NotSupportedException(param2.ToString());
        }

    }

    /// <summary>
    /// Push value in param 1 to the stack
    /// </summary>
    /// <param name="param1"></param>
    /// <exception cref="NotSupportedException"></exception>
    private void PUSH(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.BC:
                _bus.WriteMemory((ushort)(Registers.SP - 1), Registers.B);
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), Registers.C);
                _cyclesLeft--;

                Registers.SP -= 2;
                _cyclesLeft--;
                break;
            case InstructionParam.DE:
                _bus.WriteMemory((ushort)(Registers.SP - 1), Registers.D);
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), Registers.E);
                _cyclesLeft--;

                Registers.SP -= 2;
                _cyclesLeft--;
                break;
            case InstructionParam.HL:
                _bus.WriteMemory((ushort)(Registers.SP - 1), Registers.H);
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), Registers.L);
                _cyclesLeft--;

                Registers.SP -= 2;
                _cyclesLeft--;
                break;
            case InstructionParam.AF:
                _bus.WriteMemory((ushort)(Registers.SP - 1), Registers.A);
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), Registers.F);
                _cyclesLeft--;

                Registers.SP -= 2;
                _cyclesLeft--;
                break;
            case InstructionParam.PC:
                _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)((Registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)(Registers.PC & 0x00FF));
                _cyclesLeft--;

                Registers.SP -= 2;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
    }

    /// <summary>
    /// Loads value from stack pointer into provided Register
    /// </summary>
    /// <param name="param1"></param>
    /// <exception cref="NotSupportedException"></exception>
    private void POP(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.BC:
                Registers.C = _bus.ReadMemory(Registers.SP);
                _cyclesLeft--;

                Registers.B = _bus.ReadMemory((ushort)(Registers.SP + 1));
                _cyclesLeft--;

                Registers.SP += 2;
                break;
            case InstructionParam.DE:
                Registers.E = _bus.ReadMemory(Registers.SP);
                _cyclesLeft--;

                Registers.D = _bus.ReadMemory((ushort)(Registers.SP + 1));
                _cyclesLeft--;

                Registers.SP += 2;
                break;
            case InstructionParam.HL:
                Registers.L = _bus.ReadMemory(Registers.SP);
                _cyclesLeft--;

                Registers.H = _bus.ReadMemory((ushort)(Registers.SP + 1));
                _cyclesLeft--;

                Registers.SP += 2;
                break;
            case InstructionParam.AF:
                // Last 4 bits of F register are unused
                Registers.F = (byte)(_bus.ReadMemory(Registers.SP) & 0xF0);
                _cyclesLeft--;

                Registers.A = _bus.ReadMemory((ushort)(Registers.SP + 1));
                _cyclesLeft--;

                Registers.SP += 2;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
    }

    /// <summary>
    /// Moves program counter to param 2 and adds current program counter to the stack if condition param1 is met
    /// </summary>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    private void CALL(InstructionParam param1, InstructionParam param2)
    {
        var conditionMet = CheckCondition(param1);
        if (conditionMet is null)
        {
            switch (param1)
            {
                case InstructionParam.a16Mem:
                    _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)(((Registers.PC + 2)& 0xFF00) >> 8));
                    _cyclesLeft--;

                    _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)((Registers.PC + 2) & 0x00FF));
                    _cyclesLeft--;

                    var byte2 = _bus.ReadMemory(Registers.PC);
                    _cyclesLeft--;
                    Registers.PC++;

                    var byte1 = _bus.ReadMemory(Registers.PC);
                    _cyclesLeft--;
                    Registers.PC++;

                    Registers.PC = (ushort)((byte1 << 8) + byte2);
                    Registers.SP -= 2;
                    _cyclesLeft--;
                    break;
                default:
                    throw new NotSupportedException(param1.ToString());
            }

            return;
        }

        switch (param2)
        {
            case InstructionParam.a16Mem:
                if ((bool)conditionMet)
                {
                    _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)(((Registers.PC + 2) & 0xFF00) >> 8));
                    _cyclesLeft--;

                    _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)((Registers.PC + 2) & 0x00FF));
                    _cyclesLeft--;

                    var byte2 = _bus.ReadMemory(Registers.PC);
                    _cyclesLeft--;
                    Registers.PC++;

                    var byte1 = _bus.ReadMemory(Registers.PC);
                    _cyclesLeft--;
                    Registers.PC++;

                    Registers.PC = (ushort)((byte1 << 8) + byte2);
                    Registers.SP -= 2;
                    _cyclesLeft--;
                }
                else
                {
                    _cyclesLeft -= 5;

                    // Move PC so that it's pointing to the next byte of data, rather than the immediate a16 address
                    Registers.PC += 2;
                }

                break;
            default:
                throw new NotSupportedException(param2.ToString());
        }

    }

    /// <summary>
    /// Pop the value from the stack to the program counter to return to where CALL was called
    /// </summary>
    /// <param name="param1"></param>
    private void RET(InstructionParam param1)
    {
        var conditionMet = CheckCondition(param1);

        // if no parameter
        if (conditionMet is null)
        {
            var lowByte = _bus.ReadMemory(Registers.SP);
            _cyclesLeft--;

            var highByte = _bus.ReadMemory((ushort)(Registers.SP + 1));
            _cyclesLeft--;

            Registers.PC = (ushort)((highByte << 8) + lowByte);
            Registers.SP += 2;
            _cyclesLeft--;
            return;
        }

        if ((bool)conditionMet)
        {
            _cyclesLeft--;

            var lowByte = _bus.ReadMemory(Registers.SP);
            _cyclesLeft--;

            var highByte = _bus.ReadMemory((ushort)(Registers.SP + 1));
            _cyclesLeft--;

            Registers.PC = (ushort)((highByte << 8) + lowByte);
            Registers.SP += 2;
            _cyclesLeft--;
            return;
        }
    }

    /// <summary>
    /// Perform left shift on param, setting the carry bit as necessary
    /// </summary>
    /// <param name="param1"></param>
    private void RLC(InstructionParam param1)
    {
        byte bit7;
        switch (param1)
        {
            case InstructionParam.A:
                bit7 = (byte)(Registers.A >> 7);
                Registers.A = (byte)((Registers.A << 1) + bit7);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                Registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.B:
                bit7 = (byte)(Registers.B >> 7);
                Registers.B = (byte)((Registers.B << 1) + bit7);
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                Registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.C:
                bit7 = (byte)(Registers.C >> 7);
                Registers.C = (byte)((Registers.C << 1) + bit7);
                Registers.SetFlag(Flag.Zero, Registers.C == 0);
                Registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.D:
                bit7 = (byte)(Registers.D >> 7);
                Registers.D = (byte)((Registers.D << 1) + bit7);
                Registers.SetFlag(Flag.Zero, Registers.D == 0);
                Registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.E:
                bit7 = (byte)(Registers.E >> 7);
                Registers.E = (byte)((Registers.E << 1) + bit7);
                Registers.SetFlag(Flag.Zero, Registers.E == 0);
                Registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.H:
                bit7 = (byte)(Registers.H  >> 7);
                Registers.H = (byte)((Registers.H << 1) + bit7);
                Registers.SetFlag(Flag.Zero, Registers.H == 0);
                Registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.L:
                bit7 = (byte)(Registers.L >> 7);
                Registers.L = (byte)((Registers.L << 1) + bit7);
                Registers.SetFlag(Flag.Zero, Registers.L == 0);
                Registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.HLMem:
                var val = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                bit7 = (byte)(val  >> 7);
                val = (byte)((val << 1) + bit7);
                _bus.WriteMemory(Registers.HL, val);
                Registers.SetFlag(Flag.Zero, val == 0);
                Registers.SetFlag(Flag.Carry, bit7 == 1);
                _cyclesLeft--;
                break;
        }
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
    }

    private void RLCA()
    {
        var bit7 = (byte)(Registers.A  >> 7);
        Registers.A = (byte)((Registers.A << 1) + bit7);
        Registers.SetFlag(Flag.Zero, false);
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
        Registers.SetFlag(Flag.Carry, bit7 == 1);
    }

    private void RLA()
    {
        var carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0));
        Registers.SetFlag(Flag.Carry, (Registers.A & 0b10000000) == 0b10000000);
        Registers.A = (byte)((Registers.A << 1) + carryBit);
        Registers.SetFlag(Flag.Zero, false);
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
    }
    
    /// <summary>
    /// Perform right rotate on param, setting the carry bit as necessary
    /// </summary>
    /// <param name="param1"></param>
    private void RRC(InstructionParam param1)
    {
        byte bit0;
        switch (param1)
        {
            case InstructionParam.A:
                bit0 = (byte)(Registers.A & 0b1);
                Registers.A = (byte)((Registers.A >> 1) + (bit0 << 7));
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                Registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.B:
                bit0 = (byte)(Registers.B & 0b1);
                Registers.B = (byte)((Registers.B >> 1) + (bit0 << 7));
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                Registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.C:
                bit0 = (byte)(Registers.C & 0b1);
                Registers.C = (byte)((Registers.C >> 1) + (bit0 << 7));
                Registers.SetFlag(Flag.Zero, Registers.C == 0);
                Registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.D:
                bit0 = (byte)(Registers.D & 0b1);
                Registers.D = (byte)((Registers.D >> 1) + (bit0 << 7));
                Registers.SetFlag(Flag.Zero, Registers.D == 0);
                Registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.E:
                bit0 = (byte)(Registers.E & 0b1);
                Registers.E = (byte)((Registers.E >> 1) + (bit0 << 7));
                Registers.SetFlag(Flag.Zero, Registers.E == 0);
                Registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.H:
                bit0 = (byte)(Registers.H & 0b1);
                Registers.H = (byte)((Registers.H >> 1) + (bit0 << 7));
                Registers.SetFlag(Flag.Zero, Registers.H == 0);
                Registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.L:
                bit0 = (byte)(Registers.L & 0b1);
                Registers.L = (byte)((Registers.L >> 1) + (bit0 << 7));
                Registers.SetFlag(Flag.Zero, Registers.L == 0);
                Registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.HLMem:
                var val = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                bit0 = (byte)(val & 0b1);
                val = (byte)((val >> 1) + (bit0 << 7));
                _bus.WriteMemory(Registers.HL, val);
                Registers.SetFlag(Flag.Zero, val == 0);
                Registers.SetFlag(Flag.Carry, bit0 == 1);
                _cyclesLeft--;
                break;
        }

        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
    }

    private void RRCA()
    {
        var bit0 = (byte)(Registers.A & 0b1);
        Registers.A = (byte)((Registers.A >> 1) + (bit0 << 7));
        Registers.SetFlag(Flag.Zero, false);
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
        Registers.SetFlag(Flag.Carry, bit0 == 1);
    }

    private void RRA()
    {
        var carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
        Registers.SetFlag(Flag.Carry, (Registers.A & 1) == 1);
        Registers.A = (byte)((Registers.A >> 1) + carryBit);
        Registers.SetFlag(Flag.Zero, false);
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
    }

    /// <summary>
    /// Set carry flag
    /// </summary>
    private void SCF()
    {
        Registers.SetFlag(Flag.HalfCarry, false);
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.Carry, true);
    }

    /// <summary>
    /// Take the logical and for each bit with the A register and store the results in A
    /// </summary>
    /// <param name="param1"></param>
    private void AND(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.A:
                Registers.A = (byte)(Registers.A & Registers.A);
                break;
            case InstructionParam.B:
                Registers.A = (byte)(Registers.A & Registers.B);
                break;
            case InstructionParam.C:
                Registers.A = (byte)(Registers.A & Registers.C);
                break;
            case InstructionParam.D:
                Registers.A = (byte)(Registers.A & Registers.D);
                break;
            case InstructionParam.E:
                Registers.A = (byte)(Registers.A & Registers.E);
                break;
            case InstructionParam.H:
                Registers.A = (byte)(Registers.A & Registers.H);
                break;
            case InstructionParam.L:
                Registers.A = (byte)(Registers.A & Registers.L);
                break;
            case InstructionParam.HLMem:
                Registers.A = (byte)(Registers.A & _bus.ReadMemory(Registers.HL));
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                Registers.A = (byte)(Registers.A & _bus.ReadMemory(Registers.PC));
                _cyclesLeft--;
                Registers.PC++;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
        Registers.SetFlag(Flag.HalfCarry, true);
        Registers.SetFlag(Flag.Carry, false);
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.Zero, Registers.A == 0);
    }

    /// <summary>
    /// Take the logical or for each bit with the A register and store the results in A
    /// </summary>
    /// <param name="param1"></param>
    private void OR(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.A:
                Registers.A = (byte)(Registers.A | Registers.A);
                break;
            case InstructionParam.B:
                Registers.A = (byte)(Registers.A | Registers.B);
                break;
            case InstructionParam.C:
                Registers.A = (byte)(Registers.A | Registers.C);
                break;
            case InstructionParam.D:
                Registers.A = (byte)(Registers.A | Registers.D);
                break;
            case InstructionParam.E:
                Registers.A = (byte)(Registers.A | Registers.E);
                break;
            case InstructionParam.H:
                Registers.A = (byte)(Registers.A | Registers.H);
                break;
            case InstructionParam.L:
                Registers.A = (byte)(Registers.A | Registers.L);
                break;
            case InstructionParam.HLMem:
                Registers.A = (byte)(Registers.A | _bus.ReadMemory(Registers.HL));
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                Registers.A = (byte)(Registers.A | _bus.ReadMemory(Registers.PC));
                Registers.PC++;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
        Registers.SetFlag(Flag.Zero, Registers.A == 0);
        Registers.SetFlag(Flag.Carry, false);
        Registers.SetFlag(Flag.HalfCarry, false);
        Registers.SetFlag(Flag.Subtraction, false);
    }

    /// <summary>
    /// Take the logical xor for each bit with the A register and store the results in A
    /// </summary>
    /// <param name="param1"></param>
    private void XOR(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.A:
                Registers.A = (byte)(Registers.A ^ Registers.A);
                break;
            case InstructionParam.B:
                Registers.A = (byte)(Registers.A ^ Registers.B);
                break;
            case InstructionParam.C:
                Registers.A = (byte)(Registers.A ^ Registers.C);
                break;
            case InstructionParam.D:
                Registers.A = (byte)(Registers.A ^ Registers.D);
                break;
            case InstructionParam.E:
                Registers.A = (byte)(Registers.A ^ Registers.E);
                break;
            case InstructionParam.H:
                Registers.A = (byte)(Registers.A ^ Registers.H);
                break;
            case InstructionParam.L:
                Registers.A = (byte)(Registers.A ^ Registers.L);
                break;
            case InstructionParam.HLMem:
                Registers.A = (byte)(Registers.A ^ _bus.ReadMemory(Registers.HL));
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                Registers.A = (byte)(Registers.A ^ _bus.ReadMemory(Registers.PC));
                Registers.PC++;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
        Registers.SetFlag(Flag.Zero, Registers.A == 0);
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
        Registers.SetFlag(Flag.Carry, false);
    }

    /// <summary>
    /// Compare the contents of the provided param with the contents of the A register
    /// </summary>
    /// <param name="param1"></param>
    private void CP(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.A:
                Registers.SetFlag(Flag.Zero, Registers.A - Registers.A == 0);
                Registers.SetFlag(Flag.Carry, Registers.A < Registers.A);
                Registers.SetHalfCarryFlagSubtracting(Registers.A, Registers.A);
                break;
            case InstructionParam.B:
                Registers.SetFlag(Flag.Zero, Registers.A - Registers.B == 0);
                Registers.SetFlag(Flag.Carry, Registers.A < Registers.B);
                Registers.SetHalfCarryFlagSubtracting(Registers.A, Registers.B);
                break;
            case InstructionParam.C:
                Registers.SetFlag(Flag.Zero, Registers.A - Registers.C == 0);
                Registers.SetFlag(Flag.Carry, Registers.A < Registers.C);
                Registers.SetHalfCarryFlagSubtracting(Registers.A, Registers.C);
                break;
            case InstructionParam.D:
                Registers.SetFlag(Flag.Zero, Registers.A - Registers.D == 0);
                Registers.SetFlag(Flag.Carry, Registers.A < Registers.D);
                Registers.SetHalfCarryFlagSubtracting(Registers.A, Registers.D);
                break;
            case InstructionParam.E:
                Registers.SetFlag(Flag.Zero, Registers.A - Registers.E == 0);
                Registers.SetHalfCarryFlagSubtracting(Registers.A, Registers.E);
                Registers.SetFlag(Flag.Carry, Registers.A < Registers.E);
                break;
            case InstructionParam.H:
                Registers.SetFlag(Flag.Zero, Registers.A - Registers.H == 0);
                Registers.SetHalfCarryFlagSubtracting(Registers.A, Registers.H);
                Registers.SetFlag(Flag.Carry, Registers.A < Registers.H);
                break;
            case InstructionParam.L:
                Registers.SetFlag(Flag.Zero, Registers.A - Registers.L == 0);
                Registers.SetHalfCarryFlagSubtracting(Registers.A, Registers.L);
                Registers.SetFlag(Flag.Carry, Registers.A < Registers.L);
                break;
            case InstructionParam.HLMem:
                Registers.SetFlag(Flag.Zero, Registers.A - _bus.ReadMemory(Registers.HL) == 0);
                Registers.SetHalfCarryFlagSubtracting(Registers.A, _bus.ReadMemory(Registers.HL));
                Registers.SetFlag(Flag.Carry, Registers.A < _bus.ReadMemory(Registers.HL));
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                Registers.SetFlag(Flag.Zero, Registers.A - _bus.ReadMemory(Registers.PC) == 0);
                Registers.SetHalfCarryFlagSubtracting(Registers.A, _bus.ReadMemory(Registers.PC));
                Registers.SetFlag(Flag.Carry, Registers.A < _bus.ReadMemory(Registers.PC));
                Registers.PC++;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
        
        Registers.SetFlag(Flag.Subtraction, true);

    }

    /// <summary>
    /// Push the current value of the program counter PC onto the memory stack, and load into PC the [param1] byte of page 0 memory addresses, 0x00. The next instruction is fetched from the address specified by the new content of PC (as usual).
    /// </summary>
    /// <param name="param1"></param>
    /// <exception cref="NotSupportedException"></exception>
    private void RST(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.Bit0:
                _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)((Registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)(Registers.PC & 0x00FF));
                _cyclesLeft--;

                Registers.SP -= 2;
                Registers.PC = 0x0000;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit1:
                _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)((Registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)(Registers.PC & 0x00FF));
                _cyclesLeft--;

                Registers.SP -= 2;
                Registers.PC = 0x0008;
                _cyclesLeft--;
                break;

            case InstructionParam.Bit2:
                _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)((Registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)(Registers.PC & 0x00FF));
                _cyclesLeft--;

                Registers.SP -= 2;
                Registers.PC = 0x0010;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit3:
                _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)((Registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)(Registers.PC & 0x00FF));
                _cyclesLeft--;

                Registers.SP -= 2;
                Registers.PC = 0x0018;
                _cyclesLeft--;
                break;

            case InstructionParam.Bit4:
                _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)((Registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)(Registers.PC & 0x00FF));
                _cyclesLeft--;

                Registers.SP -= 2;
                Registers.PC = 0x0020;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit5:
                _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)((Registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)(Registers.PC & 0x00FF));
                _cyclesLeft--;

                Registers.SP -= 2;
                Registers.PC = 0x0028;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit6:
                _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)((Registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)(Registers.PC & 0x00FF));
                _cyclesLeft--;

                Registers.SP -= 2;
                Registers.PC = 0x0030;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit7:
                _bus.WriteMemory((ushort)(Registers.SP - 1), (byte)((Registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(Registers.SP - 2), (byte)(Registers.PC & 0x00FF));
                _cyclesLeft--;

                Registers.SP -= 2;
                Registers.PC = 0x0038;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
    }

    /// <summary>
    /// Shift the contents of param to the right.
    /// </summary>
    /// <param name="param1"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void SRL(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.A:
                Registers.SetFlag(Flag.Carry, (Registers.A & 1) == 1);
                Registers.A = (byte)(Registers.B >> 1);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.B:
                Registers.SetFlag(Flag.Carry, (Registers.B & 1) == 1);
                Registers.B = (byte)(Registers.B >> 1);
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                break;
            case InstructionParam.C:
                Registers.SetFlag(Flag.Carry, (Registers.C & 1) == 1);
                Registers.C = (byte)(Registers.C >> 1);
                Registers.SetFlag(Flag.Zero, Registers.C == 0);
                break;
            case InstructionParam.D:
                Registers.SetFlag(Flag.Carry, (Registers.D & 1) == 1);
                Registers.D = (byte)(Registers.D >> 1);
                Registers.SetFlag(Flag.Zero, Registers.D == 0);
                break;
            case InstructionParam.E:
                Registers.SetFlag(Flag.Carry, (Registers.E & 1) == 1);
                Registers.E = (byte)(Registers.E >> 1);
                Registers.SetFlag(Flag.Zero, Registers.E == 0);
                break;
            case InstructionParam.H:
                Registers.SetFlag(Flag.Carry, (Registers.H & 1) == 1);
                Registers.H = (byte)(Registers.H >> 1);
                Registers.SetFlag(Flag.Zero, Registers.H == 0);
                break;
            case InstructionParam.L:
                Registers.SetFlag(Flag.Carry, (Registers.L & 1) == 1);
                Registers.SetFlag(Flag.Zero, Registers.L == 0);
                Registers.L = (byte)(Registers.L >> 1);
                break;
            case InstructionParam.HLMem:
                var memValue = _bus.ReadMemory(Registers.HL);
                Registers.SetFlag(Flag.Carry, (memValue & 1) == 1);
                var newValue = Registers.C >> 1;
                _bus.WriteMemory(Registers.HL, (byte)(newValue));
                Registers.SetFlag(Flag.Zero, newValue == 0);
                break;
            default:
                throw new InvalidOperationException(param1.ToString());
        }
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
    }

    /// <summary>
    /// Rotates the contents of param to the right
    /// </summary>
    /// <param name="param1"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void RR(InstructionParam param1)
    {
        byte carryBit;
        switch (param1)
        {
            case InstructionParam.A:
                carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                Registers.SetFlag(Flag.Carry, (Registers.A & 1) == 1);
                Registers.A = (byte)((Registers.A >> 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.B:
                carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                Registers.SetFlag(Flag.Carry, (Registers.B & 1) == 1);
                Registers.B = (byte)((Registers.B >> 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                break;
            case InstructionParam.C:
                carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                Registers.SetFlag(Flag.Carry, (Registers.C & 1) == 1);
                Registers.C = (byte)((Registers.C >> 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.C == 0);
                break;
            case InstructionParam.D:
                carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                Registers.SetFlag(Flag.Carry, (Registers.D & 1) == 1);
                Registers.D = (byte)((Registers.D >> 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.D == 0);
                break;
            case InstructionParam.E:
                carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                Registers.SetFlag(Flag.Carry, (Registers.E & 1) == 1);
                Registers.E = (byte)((Registers.E >> 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.E == 0);
                break;
            case InstructionParam.H:
                carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                Registers.SetFlag(Flag.Carry, (Registers.H & 1) == 1);
                Registers.H = (byte)((Registers.H >> 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.H == 0);
                break;
            case InstructionParam.L:
                carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                Registers.SetFlag(Flag.Carry, (Registers.L & 1) == 1);
                Registers.L = (byte)((Registers.L >> 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.L == 0);
                break;
            case InstructionParam.HLMem:
                carryBit = (byte)((Registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);

                var memoryValue = _bus.ReadMemory(Registers.HL);
                var newValue = (byte)((Registers.A >> 1) + carryBit);

                Registers.SetFlag(Flag.Carry, (memoryValue & 1) == 1);
                _bus.WriteMemory(Registers.HL, newValue);
                Registers.SetFlag(Flag.Zero, newValue == 0);
                break;
            default:
                throw new InvalidOperationException(param1.ToString());
        }

        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
    }

    /// <summary>
    /// Rotates the contents of param to the right
    /// </summary>
    /// <param name="param1"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void RL(InstructionParam param1)
    {
        var carryBit = (byte) (Registers.GetFlag(Flag.Carry) ? 1 : 0);
        switch (param1)
        {
            case InstructionParam.A:
                Registers.SetFlag(Flag.Carry, (Registers.A & 0b10000000) > 0);
                Registers.A = (byte)((Registers.A << 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.B:
                Registers.SetFlag(Flag.Carry, (Registers.B & 0b10000000) > 0);
                Registers.B = (byte)((Registers.B << 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                break;
            case InstructionParam.C:
                Registers.SetFlag(Flag.Carry, (Registers.C & 0b10000000) > 0);
                Registers.C = (byte)((Registers.C << 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.C == 0);
                break;
            case InstructionParam.D:
                Registers.SetFlag(Flag.Carry, (Registers.D & 0b10000000) > 0);
                Registers.D = (byte)((Registers.D << 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.D == 0);
                break;
            case InstructionParam.E:
                Registers.SetFlag(Flag.Carry, (Registers.E & 0b10000000) > 0);
                Registers.E = (byte)((Registers.E << 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.E == 0);
                break;
            case InstructionParam.H:
                Registers.SetFlag(Flag.Carry, (Registers.H & 0b10000000) > 0);
                Registers.H = (byte)((Registers.H << 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.H == 0);
                break;
            case InstructionParam.L:
                Registers.SetFlag(Flag.Carry, (Registers.L & 0b10000000) > 0);
                Registers.L = (byte)((Registers.L << 1) + carryBit);
                Registers.SetFlag(Flag.Zero, Registers.L == 0);
                break;
            case InstructionParam.HLMem:
                var memoryValue = _bus.ReadMemory(Registers.HL);
                var newValue = (byte)((Registers.A << 1) + carryBit);

                Registers.SetFlag(Flag.Carry, (memoryValue & 0b10000000) > 0);
                _bus.WriteMemory(Registers.HL, newValue);
                Registers.SetFlag(Flag.Zero, newValue == 0);
                break;
            default:
                throw new InvalidOperationException(param1.ToString());
        }
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
    }

    /// <summary>
    /// Adjusts the accumulator based on Subtraction and carry flags. Used if the previous instruction is a decimal calculation
    /// </summary>
    private void DAA()
    {
        if (Registers.GetFlag(Flag.Subtraction))
        {
            if (Registers.GetFlag(Flag.Carry)) { Registers.A -= 0x60; }
            if (Registers.GetFlag(Flag.HalfCarry)) { Registers.A -= 0x6; }
        }
        else
        {
            if (Registers.GetFlag(Flag.Carry) || Registers.A > 0x99) { Registers.A += 0x60; Registers.SetFlag(Flag.Carry, true); }
            if (Registers.GetFlag(Flag.HalfCarry) || (Registers.A & 0x0f) > 0x09) { Registers.A += 0x6; }
        }
        Registers.SetFlag(Flag.Zero, Registers.A == 0);
        Registers.SetFlag(Flag.HalfCarry, false);
    }

    /// <summary>
    /// Swaps the lower order 4 bits with the higher order 4 bits of the parameter
    /// </summary>
    /// <param name="param1"></param>
    private void SWAP(InstructionParam param1)
    {
        var lowerNibble = 0;
        var upperNibble = 0;
        switch (param1)
        {
            case InstructionParam.A:
                lowerNibble = Registers.A & 0x0F;
                upperNibble = (Registers.A & 0xF0) >> 4;

                Registers.A = (byte)((lowerNibble << 4) + upperNibble);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.B:
                lowerNibble = Registers.B & 0x0F;
                upperNibble = (Registers.B & 0xF0) >> 4;

                Registers.B = (byte)((lowerNibble << 4) + upperNibble);
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                break;
            case InstructionParam.C:
                lowerNibble = Registers.C & 0x0F;
                upperNibble = (Registers.C & 0xF0) >> 4;

                Registers.C = (byte)((lowerNibble << 4) + upperNibble);
                Registers.SetFlag(Flag.Zero, Registers.C == 0);
                break;
            case InstructionParam.D:
                lowerNibble = Registers.D & 0x0F;
                upperNibble = (Registers.D & 0xF0) >> 4;

                Registers.D = (byte)((lowerNibble << 4) + upperNibble);
                Registers.SetFlag(Flag.Zero, Registers.D == 0);
                break;
            case InstructionParam.E:
                lowerNibble = Registers.E & 0x0F;
                upperNibble = (Registers.E & 0xF0) >> 4;

                Registers.E = (byte)((lowerNibble << 4) + upperNibble);
                Registers.SetFlag(Flag.Zero, Registers.E == 0);
                break;
            case InstructionParam.H:
                lowerNibble = Registers.H & 0x0F;
                upperNibble = (Registers.H & 0xF0) >> 4;

                Registers.H = (byte)((lowerNibble << 4) + upperNibble);
                Registers.SetFlag(Flag.Zero, Registers.H == 0);
                break;
            case InstructionParam.L:
                lowerNibble = Registers.L & 0x0F;
                upperNibble = (Registers.L & 0xF0) >> 4;

                Registers.L = (byte)((lowerNibble << 4) + upperNibble);
                Registers.SetFlag(Flag.Zero, Registers.L == 0);
                break;
            case InstructionParam.HLMem:
                var memVal = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                lowerNibble = memVal & 0x0F;
                upperNibble = (memVal & 0xF0) >> 4;

                _bus.WriteMemory(Registers.HL, (byte)((lowerNibble << 4) + upperNibble));
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                _cyclesLeft--;
                break;
            default: 
                throw new InvalidOperationException(param1.ToString());
        }
        Registers.SetFlag(Flag.HalfCarry, false);
        Registers.SetFlag(Flag.Carry, false);
        Registers.SetFlag(Flag.Subtraction, false);
    }

    private void HALT()
    {
        if (!_interrupts) {
            if ((_bus.ReadMemory((ushort)HardwareRegisters.IE) & _bus.ReadMemory((ushort)HardwareRegisters.IF) & 0x1F) == 0) {
                _halted = true;
                Registers.PC--;
            } else
            {
                _haltBug = true;
            }
        }
    }

    private void SLA(InstructionParam param1)
    {
        switch (param1)
        {
            case InstructionParam.A:
                Registers.SetFlag(Flag.Carry, (Registers.A & 0b10000000) > 0);
                Registers.A = (byte)(Registers.A << 1);
                Registers.SetFlag(Flag.Zero, Registers.A == 0);
                break;
            case InstructionParam.B:
                Registers.SetFlag(Flag.Carry, (Registers.B & 0b10000000) > 0);
                Registers.B = (byte)(Registers.B << 1);
                Registers.SetFlag(Flag.Zero, Registers.B == 0);
                break;
            case InstructionParam.C:
                Registers.SetFlag(Flag.Carry, (Registers.C & 0b10000000) > 0);
                Registers.C = (byte)(Registers.C << 1);
                Registers.SetFlag(Flag.Zero, Registers.C == 0);
                break;
            case InstructionParam.D:
                Registers.SetFlag(Flag.Carry, (Registers.D & 0b10000000) > 0);
                Registers.D = (byte)(Registers.D << 1);
                Registers.SetFlag(Flag.Zero, Registers.D == 0);
                break;
            case InstructionParam.E:
                Registers.SetFlag(Flag.Carry, (Registers.E & 0b10000000) > 0);
                Registers.E = (byte)(Registers.E << 1);
                Registers.SetFlag(Flag.Zero, Registers.E == 0);
                break;
            case InstructionParam.H:
                Registers.SetFlag(Flag.Carry, (Registers.H & 0b10000000) > 0);
                Registers.H = (byte)(Registers.H << 0);
                Registers.SetFlag(Flag.Zero, Registers.H == 0);
                break;
            case InstructionParam.L:
                Registers.SetFlag(Flag.Carry, (Registers.L & 0b10000000) > 0);
                Registers.L = (byte)(Registers.L << 1);
                Registers.SetFlag(Flag.Zero, Registers.L == 0);
                break;
            case InstructionParam.HLMem:
                Registers.SetFlag(Flag.Carry, (_bus.ReadMemory(Registers.HL) & 0b10000000) > 0);
                _cyclesLeft--;
                _bus.WriteMemory((ushort) Registers.HL, (byte)(_bus.ReadMemory((Registers.HL)) << 1));
                _cyclesLeft--;
                Registers.SetFlag(Flag.Zero, _bus.ReadMemory(Registers.HL) == 0);
                break;
            default:
                throw new InvalidOperationException(param1.ToString());
        }
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, false);
    }
    
    private void SRA(InstructionParam param1)
        {
            switch (param1)
            {
                case InstructionParam.A:
                    Registers.SetFlag(Flag.Carry, (Registers.A & 1) > 0);
                    Registers.A = (byte)(Registers.A >> 1);
                    Registers.SetFlag(Flag.Zero, Registers.A == 0);
                    break;
                case InstructionParam.B:
                    Registers.SetFlag(Flag.Carry, (Registers.B & 1) > 0);
                    Registers.B = (byte)(Registers.B >> 1);
                    Registers.SetFlag(Flag.Zero, Registers.B == 0);
                    break;
                case InstructionParam.C:
                    Registers.SetFlag(Flag.Carry, (Registers.C & 1) > 0);
                    Registers.C = (byte)(Registers.C >> 1);
                    Registers.SetFlag(Flag.Zero, Registers.C == 0);
                    break;
                case InstructionParam.D:
                    Registers.SetFlag(Flag.Carry, (Registers.D & 1) > 0);
                    Registers.D = (byte)(Registers.D >> 1);
                    Registers.SetFlag(Flag.Zero, Registers.D == 0);
                    break;
                case InstructionParam.E:
                    Registers.SetFlag(Flag.Carry, (Registers.E & 1) > 0);
                    Registers.E = (byte)(Registers.E >> 1);
                    Registers.SetFlag(Flag.Zero, Registers.E == 0);
                    break;
                case InstructionParam.H:
                    Registers.SetFlag(Flag.Carry, (Registers.H & 1) > 0);
                    Registers.H = (byte)(Registers.H >> 0);
                    Registers.SetFlag(Flag.Zero, Registers.H == 0);
                    break;
                case InstructionParam.L:
                    Registers.SetFlag(Flag.Carry, (Registers.L & 1) > 0);
                    Registers.L = (byte)(Registers.L >> 1);
                    Registers.SetFlag(Flag.Zero, Registers.L == 0);
                    break;
                case InstructionParam.HLMem:
                    Registers.SetFlag(Flag.Carry, (_bus.ReadMemory(Registers.HL) & 1) > 0);
                    _cyclesLeft--;
                    _bus.WriteMemory( Registers.HL, (byte)(_bus.ReadMemory((Registers.HL)) >> 1));
                    Registers.SetFlag(Flag.Zero, _bus.ReadMemory(Registers.HL) == 0);
                    _cyclesLeft--;
                    break;
                default:
                    throw new InvalidOperationException(param1.ToString());
            }
            Registers.SetFlag(Flag.Subtraction, false);
            Registers.SetFlag(Flag.HalfCarry, false);
        }

    private void RES(InstructionParam param1, InstructionParam param2)
    {
        byte bitToReset = param1 switch
        {
            InstructionParam.Bit0 => 0b11111110,
            InstructionParam.Bit1 => 0b11111101,
            InstructionParam.Bit2 => 0b11111011,
            InstructionParam.Bit3 => 0b11110111,
            InstructionParam.Bit4 => 0b11101111,
            InstructionParam.Bit5 => 0b11011111,
            InstructionParam.Bit6 => 0b10111111,
            InstructionParam.Bit7 => 0b01111111,
            _ => throw new InvalidOperationException(param1.ToString())
        };

        switch (param2)
        {
            case InstructionParam.A:
                Registers.A &= bitToReset;
                break;
            case InstructionParam.B:
                Registers.B &= bitToReset;
                break;
            case InstructionParam.C:
                Registers.C &= bitToReset;
                break;
            case InstructionParam.D:
                Registers.D &= bitToReset;
                break;
            case InstructionParam.E:
                Registers.E &= bitToReset;
                break;
            case InstructionParam.H:
                Registers.H &= bitToReset;
                break;
            case InstructionParam.L:
                Registers.L &= bitToReset;
                break;
            case InstructionParam.HLMem:
                var valueToUpdate = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                valueToUpdate  &= bitToReset;
                
                _bus.WriteMemory(Registers.HL, valueToUpdate);
                _cyclesLeft--;
                break;
            default:
                throw new InvalidOperationException(param2.ToString());
        }
    }
    
    private void SET(InstructionParam param1, InstructionParam param2)
    {
        byte bitToSet = param1 switch
        {
            InstructionParam.Bit0 => 0b00000001,
            InstructionParam.Bit1 => 0b00000010,
            InstructionParam.Bit2 => 0b00000100,
            InstructionParam.Bit3 => 0b00001000,
            InstructionParam.Bit4 => 0b00010000,
            InstructionParam.Bit5 => 0b00100000,
            InstructionParam.Bit6 => 0b01000000,
            InstructionParam.Bit7 => 0b10000000,
            _ => throw new InvalidOperationException(param1.ToString())
        };

        switch (param2)
        {
            case InstructionParam.A:
                Registers.A |= bitToSet;
                break;
            case InstructionParam.B:
                Registers.B |= bitToSet;
                break;
            case InstructionParam.C:
                Registers.C |= bitToSet;
                break;
            case InstructionParam.D:
                Registers.D |= bitToSet;
                break;
            case InstructionParam.E:
                Registers.E |= bitToSet;
                break;
            case InstructionParam.H:
                Registers.H |= bitToSet;
                break;
            case InstructionParam.L:
                Registers.L |= bitToSet;
                break;
            case InstructionParam.HLMem:
                var valueToUpdate = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                valueToUpdate  |= bitToSet;
                
                _bus.WriteMemory(Registers.HL, valueToUpdate);
                _cyclesLeft--;
                break;
            default:
                throw new InvalidOperationException(param2.ToString());
        }
    }
    
    /// <summary>
    /// Copies the complement (i.e. the opposite) of the specified bit from the specified register to the z flag
    /// </summary>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void BIT(InstructionParam param1, InstructionParam param2)
    {
        byte bitToCopy = param1 switch
        {
            InstructionParam.Bit0 => 0b00000001,
            InstructionParam.Bit1 => 0b00000010,
            InstructionParam.Bit2 => 0b00000100,
            InstructionParam.Bit3 => 0b00001000,
            InstructionParam.Bit4 => 0b00010000,
            InstructionParam.Bit5 => 0b00100000,
            InstructionParam.Bit6 => 0b01000000,
            InstructionParam.Bit7 => 0b10000000,
            _ => throw new InvalidOperationException(param1.ToString())
        };
        bool bitSet;
        switch (param2)
        {
            case InstructionParam.A:
                bitSet = (Registers.A & bitToCopy) > 0;
                break;
            case InstructionParam.B:
                bitSet = (Registers.B & bitToCopy) > 0;
                break;
            case InstructionParam.C:
                bitSet = (Registers.C & bitToCopy) > 0;
                break;
            case InstructionParam.D:
                bitSet = (Registers.D & bitToCopy) > 0;
                break;
            case InstructionParam.E:
                bitSet = (Registers.E & bitToCopy) > 0;
                break;
            case InstructionParam.H:
                bitSet = (Registers.H & bitToCopy) > 0;
                break;
            case InstructionParam.L:
                bitSet = (Registers.L & bitToCopy) > 0;
                break;
            case InstructionParam.HLMem:
                var memoryValue = _bus.ReadMemory(Registers.HL);
                _cyclesLeft--;
                bitSet = (memoryValue & bitToCopy) > 0;
                
                break;
            default:
                throw new InvalidOperationException(param2.ToString());
        }
        Registers.SetFlag(Flag.Zero,  !bitSet );
        Registers.SetFlag(Flag.Subtraction, false);
        Registers.SetFlag(Flag.HalfCarry, true);
    }

    private bool? CheckCondition(InstructionParam condition)
    {
        bool? conditionMet;
        switch (condition)
        {
            case InstructionParam.NZ:
                conditionMet = !Registers.GetFlag(Flag.Zero);
                break;
            case InstructionParam.NC:
                conditionMet = !Registers.GetFlag(Flag.Carry);
                break;
            case InstructionParam.Z:
                conditionMet = Registers.GetFlag(Flag.Zero);
                break;
            case InstructionParam.C:
                conditionMet = Registers.GetFlag(Flag.Carry);
                break;
            default:
                // return null if not a condition
                conditionMet = null;
                break;
        }

        return conditionMet;
    }

}