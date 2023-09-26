namespace GBEmulator.Hardware;

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
                data = _registers.A;
                break;
            case InstructionParam.B:
                data = _registers.B;
                break;
            case InstructionParam.C:
                data = _registers.C;
                break;
            case InstructionParam.D:
                data = _registers.D;
                break;
            case InstructionParam.E:
                data = _registers.E;
                break;
            case InstructionParam.L:
                data = _registers.L;
                break;
            case InstructionParam.H:
                data = _registers.H;
                break;
            case InstructionParam.HL:
                extraData = _registers.H;
                data = _registers.L;
                break;
            case InstructionParam.d8:
                data = _bus.ReadMemory(_registers.PC);
                _registers.PC++;
                _cyclesLeft--;
                break;
            case InstructionParam.d16:
                data = _bus.ReadMemory(_registers.PC);
                _registers.PC++;
                _cyclesLeft--;
                extraData = _bus.ReadMemory(_registers.PC);
                _registers.PC++;
                _cyclesLeft--;
                break;
            case InstructionParam.BCMem:
                data = _bus.ReadMemory(_registers.BC);
                _cyclesLeft--;
                break;
            case InstructionParam.DEMem:
                data = _bus.ReadMemory(_registers.DE);
                _cyclesLeft--;
                break;
            case InstructionParam.HLMem:
                data = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                break;
            case InstructionParam.HLIMem:
                data = _bus.ReadMemory(_registers.HL);
                _registers.HL++;
                _cyclesLeft--;
                break;
            case InstructionParam.HLDMem:
                data = _bus.ReadMemory(_registers.HL);
                _registers.HL--;
                _cyclesLeft--;
                break;
            case InstructionParam.CMem:
                addressToRead = (ushort)(0xFF00 + _registers.C);
                data = _bus.ReadMemory(addressToRead);
                _cyclesLeft--;
                break;
            case InstructionParam.a8Mem:
                addressToRead = (ushort)(0xFF00 + _bus.ReadMemory(_registers.PC));
                _registers.PC++;
                _cyclesLeft--;

                data = _bus.ReadMemory(addressToRead);
                _cyclesLeft--;
                break;
            case InstructionParam.SP:
                data = (byte)(_registers.SP & 0x00FF);
                extraData = (byte)((_registers.SP & 0xFF00) >> 8);
                break;
            case InstructionParam.a16Mem:
                var lowByte = _bus.ReadMemory(_registers.PC);
                _registers.PC++;
                _cyclesLeft--;
                var highByte = _bus.ReadMemory(_registers.PC);
                _registers.PC++;
                _cyclesLeft--;

                addressToRead = (ushort)((highByte << 8) + lowByte);

                data = _bus.ReadMemory(addressToRead);
                _cyclesLeft--;
                break;
            case InstructionParam.SPs8:
                _registers.SetFlag(Flag.Zero, false);
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetCarryFlags8Bit((sbyte)_bus.ReadMemory(_registers.PC), _registers.SP);
                var calculatedVal = (sbyte)_bus.ReadMemory(_registers.PC) + _registers.SP;
                data = (byte)calculatedVal;
                extraData = (byte)((calculatedVal & 0xFF00) >> 8);
                _registers.PC++;
                _cyclesLeft -= 2;
                break;
            default:
                throw new InvalidOperationException(nameof(dataToLoad));
        }

        ushort addressToWrite;
        switch (loadTo)
        {
            case InstructionParam.A:
                _registers.A = data;
                break;
            case InstructionParam.B:
                _registers.B = data;
                break;
            case InstructionParam.C:
                _registers.C = data;
                break;
            case InstructionParam.D:
                _registers.D = data;
                break;
            case InstructionParam.E:
                _registers.E = data;
                break;
            case InstructionParam.L:
                _registers.L = data;
                break;
            case InstructionParam.H:
                _registers.H = data;
                break;
            case InstructionParam.BCMem:
                _bus.WriteMemory(_registers.BC, data);
                _cyclesLeft--;
                break;
            case InstructionParam.DEMem:
                _bus.WriteMemory(_registers.DE, data);
                _cyclesLeft--;
                break;
            case InstructionParam.HLMem:
                _bus.WriteMemory(_registers.HL, data);
                _cyclesLeft--;
                break;
            case InstructionParam.HLIMem:
                _bus.WriteMemory(_registers.HL, data);
                _registers.HL++;
                _cyclesLeft--;
                break;
            case InstructionParam.HLDMem:
                _bus.WriteMemory(_registers.HL, data);
                _registers.HL--;
                _cyclesLeft--;
                break;
            case InstructionParam.BC:
                _registers.C = data;
                _registers.B = extraData;
                break;
            case InstructionParam.DE:
                _registers.E = data;
                _registers.D = extraData;
                break;
            case InstructionParam.HL:
                _registers.L = data;
                _registers.H = extraData;
                break;
            case InstructionParam.SP:
                _registers.SP = (ushort)((extraData << 8) + data);
                break;
            case InstructionParam.CMem:
                addressToWrite = (ushort)(0xFF00 + _registers.C);
                _bus.WriteMemory(addressToWrite, data);
                _cyclesLeft--;
                break;
            case InstructionParam.a8Mem:
                addressToWrite = (ushort)(0xFF00 + _bus.ReadMemory(_registers.PC));
                _cyclesLeft--;
                _registers.PC++;

                _bus.WriteMemory(addressToWrite, data);
                _cyclesLeft--;
                break;
            case InstructionParam.a16Mem:
                var lowByte = _bus.ReadMemory(_registers.PC);
                _registers.PC++;
                _cyclesLeft--;
                var highByte = _bus.ReadMemory(_registers.PC);
                _registers.PC++;
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
                valueToAdd = _registers.A;
                break;
            case InstructionParam.B:
                valueToAdd = _registers.B;
                break;
            case InstructionParam.C:
                valueToAdd = _registers.C;
                break;
            case InstructionParam.D:
                valueToAdd = _registers.D;
                break;
            case InstructionParam.E:
                valueToAdd = _registers.E;
                break;
            case InstructionParam.H:
                valueToAdd = _registers.H;
                break;
            case InstructionParam.L:
                valueToAdd = _registers.L;
                break;
            case InstructionParam.BC:
                valueToAdd = _registers.BC;
                break;
            case InstructionParam.DE:
                valueToAdd = _registers.DE;
                break;
            case InstructionParam.HL:
                valueToAdd = _registers.HL;
                break;
            case InstructionParam.SP:
                valueToAdd = _registers.SP;
                break;
            case InstructionParam.HLMem:
                valueToAdd = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                valueToAdd = _bus.ReadMemory(_registers.PC);
                _cyclesLeft--;
                _registers.PC++;
                break;
            case InstructionParam.s8:
                valueToAdd = (sbyte)_bus.ReadMemory(_registers.PC);
                _cyclesLeft--;
                _registers.PC++;
                break;
            default:
                throw new NotSupportedException(paramToAdd.ToString());
        }

        switch (paramToAddTo)
        {
            case InstructionParam.A:
                _registers.SetCarryFlags8Bit(_registers.A, valueToAdd);
                _registers.A = (byte)(valueToAdd + _registers.A);
                _registers.SetFlag(Flag.Zero, _registers.A == 0x00);
                break;
            case InstructionParam.HL:
                _registers.SetCarryFlags2shorts(_registers.HL, (ushort)valueToAdd);
                _registers.HL += (ushort)valueToAdd;
                _cyclesLeft--;
                break;
            case InstructionParam.SP:
                _registers.SetCarryFlags8Bit(_registers.SP, valueToAdd);
                _registers.SetFlag(Flag.Zero, false);
                _registers.SP = (ushort)(_registers.SP + (sbyte)valueToAdd);
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(paramToAdd.ToString());
        }
        _registers.SetFlag(Flag.Subtraction, false);
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
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.A & 0xf)) & 0x10) != 0 );
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.A & 0xff)) & 0x100) != 0 );
                _registers.A = (byte)(_registers.A - _registers.A);
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.B & 0xf)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.B & 0xff)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.B);
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.C:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.C & 0xf)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.C & 0xff)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.C);
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.D:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.D & 0xf)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.D & 0xff)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.D);
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.E:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.E & 0xf)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.E & 0xff)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.E);
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.H:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.H & 0xf)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.H & 0xff)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.H);
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.L:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.L & 0xf)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.L & 0xff)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.L);
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.HLMem:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_bus.ReadMemory(_registers.HL) & 0xf)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_bus.ReadMemory(_registers.HL))) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _bus.ReadMemory(_registers.HL));
                _cyclesLeft--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.d8:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_bus.ReadMemory(_registers.PC) & 0xf)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_bus.ReadMemory(_registers.PC))) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _bus.ReadMemory(_registers.PC));
                _registers.PC++;
                _cyclesLeft--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
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
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.A & 0xf) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.A & 0xff) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.A - (_registers.GetFlag(Flag.Carry) ? 1 : 0));
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.B & 0xf) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.B & 0xff) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.B - (_registers.GetFlag(Flag.Carry) ? 1 : 0));
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.C:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.C & 0xf) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.C & 0xff) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.C - (_registers.GetFlag(Flag.Carry) ? 1 : 0));
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.D:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.D & 0xf) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.D & 0xff) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.D - (_registers.GetFlag(Flag.Carry) ? 1 : 0));
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.E:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.E & 0xf) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.E & 0xff) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.E - (_registers.GetFlag(Flag.Carry) ? 1 : 0));
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.H:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.H & 0xf) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.H & 0xff) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.H - (_registers.GetFlag(Flag.Carry) ? 1 : 0));
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.L:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_registers.L & 0xf) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_registers.L & 0xff) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _registers.L - (_registers.GetFlag(Flag.Carry) ? 1 : 0));
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.HLMem:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_bus.ReadMemory(_registers.HL) & 0xf) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_bus.ReadMemory(_registers.HL)) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _bus.ReadMemory(_registers.HL) - (_registers.GetFlag(Flag.Carry) ? 1 : 0));
                _cyclesLeft--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.d8:
                _registers.SetFlag(Flag.HalfCarry, (((_registers.A & 0xf) - (_bus.ReadMemory(_registers.PC) & 0xf) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x10) != 0);
                _registers.SetFlag(Flag.Carry, (((_registers.A & 0xff) - (_bus.ReadMemory(_registers.PC)) - (_registers.GetFlag(Flag.Carry) ? 1 : 0)) & 0x100) != 0);
                _registers.A = (byte)(_registers.A - _bus.ReadMemory(_registers.PC) - (_registers.GetFlag(Flag.Carry) ? 1 : 0));
                _registers.PC++;
                _cyclesLeft--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
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
        var carryValue = _registers.GetFlag(Flag.Carry) ? 1 : 0;
        byte valueToAdd;
        switch (paramToAdd)
        {
            case InstructionParam.A:
                valueToAdd = _registers.A;
                break;          
            case InstructionParam.B:
                valueToAdd = _registers.B;
                break;
            case InstructionParam.C:
                valueToAdd = _registers.C;
                break;
            case InstructionParam.D:
                valueToAdd = _registers.D;
                break;
            case InstructionParam.E:
                valueToAdd = _registers.E;
                break;
            case InstructionParam.H:
                valueToAdd = _registers.H;
                break;
            case InstructionParam.L:
                valueToAdd = _registers.B;
                break;
            case InstructionParam.HLMem:
                valueToAdd = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                valueToAdd = _bus.ReadMemory(_registers.PC);
                _registers.PC++;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(paramToAdd.ToString());
        }

        switch (paramToAddTo)
        {
            case InstructionParam.A:
                var carryOverride = false;
                if (_registers.GetFlag(Flag.Carry))
                {
                    _registers.A++;
                    if (_registers.A == 0)
                    {
                        carryOverride = true;
                    }
                }
                _registers.SetCarryFlags8Bit(_registers.A, valueToAdd);
                
                if(carryOverride) _registers.SetFlag(Flag.Carry, true);
                
                _registers.A += valueToAdd;
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                _registers.SetFlag(Flag.Subtraction, false);
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
                _registers.SetHalfCarryFlag(_registers.A, 1);
                _registers.A++;
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                _registers.SetHalfCarryFlag(_registers.B, 1);
                _registers.B++;
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                break;
            case InstructionParam.C:
                _registers.SetHalfCarryFlag(_registers.C, 1);
                _registers.C++;
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                break;
            case InstructionParam.D:
                _registers.SetHalfCarryFlag(_registers.D, 1);
                _registers.D++;
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                break;
            case InstructionParam.E:
                _registers.SetHalfCarryFlag(_registers.E, 1);
                _registers.E++;
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                break;
            case InstructionParam.H:
                _registers.SetHalfCarryFlag(_registers.H, 1);
                _registers.H++;
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                break;
            case InstructionParam.L:
                _registers.SetHalfCarryFlag(_registers.L, 1);
                _registers.L++;
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                break;
            case InstructionParam.BC:
                _registers.BC++;
                _cyclesLeft--;
                break;
            case InstructionParam.DE:
                _registers.DE++;
                _cyclesLeft--;
                break;
            case InstructionParam.HL:
                _registers.HL++;
                _cyclesLeft--;
                break;
            case InstructionParam.SP:
                _registers.SP++;
                _cyclesLeft--;
                break;
            case InstructionParam.HLMem:
                var valueToAddTo = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                _registers.SetHalfCarryFlag(valueToAddTo, 1);
                valueToAddTo++;
                _bus.WriteMemory(_registers.HL, valueToAddTo);
                _cyclesLeft--;
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Zero, valueToAddTo == 0);
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
                _registers.SetHalfCarryFlagSubtracting(_registers.A, 1);
                _registers.A--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                _registers.SetHalfCarryFlagSubtracting(_registers.B, 1);
                _registers.B--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                break;
            case InstructionParam.C:
                _registers.SetHalfCarryFlagSubtracting(_registers.C, 1);
                _registers.C--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                break;
            case InstructionParam.D:
                _registers.SetHalfCarryFlagSubtracting(_registers.D, 1);
                _registers.D--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                break;
            case InstructionParam.E:
                _registers.SetHalfCarryFlagSubtracting(_registers.E, 1);
                _registers.E--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                break;
            case InstructionParam.H:
                _registers.SetHalfCarryFlagSubtracting(_registers.H, 1);
                _registers.H--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                break;
            case InstructionParam.L:
                _registers.SetHalfCarryFlagSubtracting(_registers.L, 1);
                _registers.L--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                break;
            case InstructionParam.BC:
                _registers.BC--;
                _cyclesLeft--;
                break;
            case InstructionParam.DE:
                _registers.DE--;
                _cyclesLeft--;
                break;
            case InstructionParam.HL:
                _registers.HL--;
                _cyclesLeft--;
                break;
            case InstructionParam.SP:
                _registers.SP--;
                _cyclesLeft--;
                break;
            case InstructionParam.HLMem:
                var valueToDecrement = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                _registers.SetHalfCarryFlagSubtracting(valueToDecrement, 1);
                valueToDecrement--;
                _bus.WriteMemory(_registers.HL, valueToDecrement);
                _cyclesLeft--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, valueToDecrement == 0);
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
                    var lowByte = _bus.ReadMemory(_registers.PC);
                    _registers.PC++;
                    _cyclesLeft--;

                    var highByte = _bus.ReadMemory(_registers.PC);
                    _registers.PC++;
                    _cyclesLeft--;

                    var address = (ushort)((highByte << 8) + lowByte);
                    _registers.PC = address;
                    _cyclesLeft--;
                    break;
                case InstructionParam.HL:
                    _registers.PC = _registers.HL;
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
                    var lowByte = _bus.ReadMemory(_registers.PC);
                    _registers.PC++;
                    _cyclesLeft--;

                    var highByte = _bus.ReadMemory(_registers.PC);
                    _registers.PC++;
                    _cyclesLeft--;

                    var address = (ushort)((highByte << 8) + lowByte);
                    _registers.PC = address;
                    _cyclesLeft--;
                }
                else
                {
                    _cyclesLeft -= 3;
                    _registers.PC += 2;
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
            _registers.PC = (ushort)(steps + _registers.PC);
            _cyclesLeft--;
        }

        var conditionMet = CheckCondition(param1);
        if (conditionMet is null)
        {
            Jump((sbyte)_bus.ReadMemory(_registers.PC));
            _registers.PC++;
            _cyclesLeft--;
            return;
        }

        switch (param2)
        {
            case InstructionParam.s8:
                if ((bool)conditionMet)
                {
                    Jump((sbyte)_bus.ReadMemory(_registers.PC));
                    _registers.PC++;
                    _cyclesLeft--;
                }
                else
                {
                    _cyclesLeft -= 2;
                    _registers.PC++;
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
                _bus.WriteMemory((ushort)(_registers.SP - 1), _registers.B);
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), _registers.C);
                _cyclesLeft--;

                _registers.SP -= 2;
                _cyclesLeft--;
                break;
            case InstructionParam.DE:
                _bus.WriteMemory((ushort)(_registers.SP - 1), _registers.D);
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), _registers.E);
                _cyclesLeft--;

                _registers.SP -= 2;
                _cyclesLeft--;
                break;
            case InstructionParam.HL:
                _bus.WriteMemory((ushort)(_registers.SP - 1), _registers.H);
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), _registers.L);
                _cyclesLeft--;

                _registers.SP -= 2;
                _cyclesLeft--;
                break;
            case InstructionParam.AF:
                _bus.WriteMemory((ushort)(_registers.SP - 1), _registers.A);
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), _registers.F);
                _cyclesLeft--;

                _registers.SP -= 2;
                _cyclesLeft--;
                break;
            case InstructionParam.PC:
                _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
                _cyclesLeft--;

                _registers.SP -= 2;
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
                _registers.C = _bus.ReadMemory(_registers.SP);
                _cyclesLeft--;

                _registers.B = _bus.ReadMemory((ushort)(_registers.SP + 1));
                _cyclesLeft--;

                _registers.SP += 2;
                break;
            case InstructionParam.DE:
                _registers.E = _bus.ReadMemory(_registers.SP);
                _cyclesLeft--;

                _registers.D = _bus.ReadMemory((ushort)(_registers.SP + 1));
                _cyclesLeft--;

                _registers.SP += 2;
                break;
            case InstructionParam.HL:
                _registers.L = _bus.ReadMemory(_registers.SP);
                _cyclesLeft--;

                _registers.H = _bus.ReadMemory((ushort)(_registers.SP + 1));
                _cyclesLeft--;

                _registers.SP += 2;
                break;
            case InstructionParam.AF:
                // Last 4 bits of F register are unused
                _registers.F = (byte)(_bus.ReadMemory(_registers.SP) & 0xF0);
                _cyclesLeft--;

                _registers.A = _bus.ReadMemory((ushort)(_registers.SP + 1));
                _cyclesLeft--;

                _registers.SP += 2;
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
                    _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)(((_registers.PC + 2)& 0xFF00) >> 8));
                    _cyclesLeft--;

                    _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)((_registers.PC + 2) & 0x00FF));
                    _cyclesLeft--;

                    var byte2 = _bus.ReadMemory(_registers.PC);
                    _cyclesLeft--;
                    _registers.PC++;

                    var byte1 = _bus.ReadMemory(_registers.PC);
                    _cyclesLeft--;
                    _registers.PC++;

                    _registers.PC = (ushort)((byte1 << 8) + byte2);
                    _registers.SP -= 2;
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
                    _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)(((_registers.PC + 2) & 0xFF00) >> 8));
                    _cyclesLeft--;

                    _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)((_registers.PC + 2) & 0x00FF));
                    _cyclesLeft--;

                    var byte2 = _bus.ReadMemory(_registers.PC);
                    _cyclesLeft--;
                    _registers.PC++;

                    var byte1 = _bus.ReadMemory(_registers.PC);
                    _cyclesLeft--;
                    _registers.PC++;

                    _registers.PC = (ushort)((byte1 << 8) + byte2);
                    _registers.SP -= 2;
                    _cyclesLeft--;
                }
                else
                {
                    _cyclesLeft -= 5;

                    // Move PC so that it's pointing to the next byte of data, rather than the immediate a16 address
                    _registers.PC += 2;
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
            var lowByte = _bus.ReadMemory(_registers.SP);
            _cyclesLeft--;

            var highByte = _bus.ReadMemory((ushort)(_registers.SP + 1));
            _cyclesLeft--;

            _registers.PC = (ushort)((highByte << 8) + lowByte);
            _registers.SP += 2;
            _cyclesLeft--;
            return;
        }

        if ((bool)conditionMet)
        {
            _cyclesLeft--;

            var lowByte = _bus.ReadMemory(_registers.SP);
            _cyclesLeft--;

            var highByte = _bus.ReadMemory((ushort)(_registers.SP + 1));
            _cyclesLeft--;

            _registers.PC = (ushort)((highByte << 8) + lowByte);
            _registers.SP += 2;
            _cyclesLeft--;
            return;
        }
        else
        {
            _cyclesLeft -= 4;
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
                bit7 = (byte)(_registers.A >> 7);
                _registers.A = (byte)((_registers.A << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.B:
                bit7 = (byte)(_registers.B >> 7);
                _registers.B = (byte)((_registers.B << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.C:
                bit7 = (byte)(_registers.C >> 7);
                _registers.C = (byte)((_registers.C << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.D:
                bit7 = (byte)(_registers.D >> 7);
                _registers.D = (byte)((_registers.D << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.E:
                bit7 = (byte)(_registers.E >> 7);
                _registers.E = (byte)((_registers.E << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.H:
                bit7 = (byte)(_registers.H  >> 7);
                _registers.H = (byte)((_registers.H << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.L:
                bit7 = (byte)(_registers.L >> 7);
                _registers.L = (byte)((_registers.L << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.HLMem:
                var val = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                bit7 = (byte)(val  >> 7);
                val = (byte)((val << 1) + bit7);
                _bus.WriteMemory(_registers.HL, val);
                _registers.SetFlag(Flag.Zero, val == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                _cyclesLeft--;
                break;
        }
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
    }

    private void RLCA()
    {
        var bit7 = (byte)(_registers.A  >> 7);
        _registers.A = (byte)((_registers.A << 1) + bit7);
        _registers.SetFlag(Flag.Zero, false);
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
        _registers.SetFlag(Flag.Carry, bit7 == 1);
    }

    private void RLA()
    {
        var carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0));
        _registers.SetFlag(Flag.Carry, (_registers.A & 0b10000000) == 0b10000000);
        _registers.A = (byte)((_registers.A << 1) + carryBit);
        _registers.SetFlag(Flag.Zero, false);
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
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
                bit0 = (byte)(_registers.A & 0b1);
                _registers.A = (byte)((_registers.A >> 1) + (bit0 << 7));
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.B:
                bit0 = (byte)(_registers.B & 0b1);
                _registers.B = (byte)((_registers.B >> 1) + (bit0 << 7));
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.C:
                bit0 = (byte)(_registers.C & 0b1);
                _registers.C = (byte)((_registers.C >> 1) + (bit0 << 7));
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.D:
                bit0 = (byte)(_registers.D & 0b1);
                _registers.D = (byte)((_registers.D >> 1) + (bit0 << 7));
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.E:
                bit0 = (byte)(_registers.E & 0b1);
                _registers.E = (byte)((_registers.E >> 1) + (bit0 << 7));
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.H:
                bit0 = (byte)(_registers.H & 0b1);
                _registers.H = (byte)((_registers.H >> 1) + (bit0 << 7));
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.L:
                bit0 = (byte)(_registers.L & 0b1);
                _registers.L = (byte)((_registers.L >> 1) + (bit0 << 7));
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.HLMem:
                var val = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                bit0 = (byte)(val & 0b1);
                val = (byte)((val >> 1) + (bit0 << 7));
                _bus.WriteMemory(_registers.HL, val);
                _registers.SetFlag(Flag.Zero, val == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                _cyclesLeft--;
                break;
        }

        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
    }

    private void RRCA()
    {
        var bit0 = (byte)(_registers.A & 0b1);
        _registers.A = (byte)((_registers.A >> 1) + (bit0 << 7));
        _registers.SetFlag(Flag.Zero, false);
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
        _registers.SetFlag(Flag.Carry, bit0 == 1);
    }

    private void RRA()
    {
        var carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
        _registers.SetFlag(Flag.Carry, (_registers.A & 1) == 1);
        _registers.A = (byte)((_registers.A >> 1) + carryBit);
        _registers.SetFlag(Flag.Zero, false);
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
    }

    /// <summary>
    /// Set carry flag
    /// </summary>
    private void SCF()
    {
        _registers.SetFlag(Flag.HalfCarry, false);
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.Carry, true);
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
                _registers.A = (byte)(_registers.A & _registers.A);
                break;
            case InstructionParam.B:
                _registers.A = (byte)(_registers.A & _registers.B);
                break;
            case InstructionParam.C:
                _registers.A = (byte)(_registers.A & _registers.C);
                break;
            case InstructionParam.D:
                _registers.A = (byte)(_registers.A & _registers.D);
                break;
            case InstructionParam.E:
                _registers.A = (byte)(_registers.A & _registers.E);
                break;
            case InstructionParam.H:
                _registers.A = (byte)(_registers.A & _registers.H);
                break;
            case InstructionParam.L:
                _registers.A = (byte)(_registers.A & _registers.L);
                break;
            case InstructionParam.HLMem:
                _registers.A = (byte)(_registers.A & _bus.ReadMemory(_registers.HL));
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                _registers.A = (byte)(_registers.A & _bus.ReadMemory(_registers.PC));
                _cyclesLeft--;
                _registers.PC++;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
        _registers.SetFlag(Flag.HalfCarry, true);
        _registers.SetFlag(Flag.Carry, false);
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.Zero, _registers.A == 0);
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
                _registers.A = (byte)(_registers.A | _registers.A);
                break;
            case InstructionParam.B:
                _registers.A = (byte)(_registers.A | _registers.B);
                break;
            case InstructionParam.C:
                _registers.A = (byte)(_registers.A | _registers.C);
                break;
            case InstructionParam.D:
                _registers.A = (byte)(_registers.A | _registers.D);
                break;
            case InstructionParam.E:
                _registers.A = (byte)(_registers.A | _registers.E);
                break;
            case InstructionParam.H:
                _registers.A = (byte)(_registers.A | _registers.H);
                break;
            case InstructionParam.L:
                _registers.A = (byte)(_registers.A | _registers.L);
                break;
            case InstructionParam.HLMem:
                _registers.A = (byte)(_registers.A | _bus.ReadMemory(_registers.HL));
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                _registers.A = (byte)(_registers.A | _bus.ReadMemory(_registers.PC));
                _registers.PC++;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
        _registers.SetFlag(Flag.Zero, _registers.A == 0);
        _registers.SetFlag(Flag.Carry, false);
        _registers.SetFlag(Flag.HalfCarry, false);
        _registers.SetFlag(Flag.Subtraction, false);
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
                _registers.A = (byte)(_registers.A ^ _registers.A);
                break;
            case InstructionParam.B:
                _registers.A = (byte)(_registers.A ^ _registers.B);
                break;
            case InstructionParam.C:
                _registers.A = (byte)(_registers.A ^ _registers.C);
                break;
            case InstructionParam.D:
                _registers.A = (byte)(_registers.A ^ _registers.D);
                break;
            case InstructionParam.E:
                _registers.A = (byte)(_registers.A ^ _registers.E);
                break;
            case InstructionParam.H:
                _registers.A = (byte)(_registers.A ^ _registers.H);
                break;
            case InstructionParam.L:
                _registers.A = (byte)(_registers.A ^ _registers.L);
                break;
            case InstructionParam.HLMem:
                _registers.A = (byte)(_registers.A ^ _bus.ReadMemory(_registers.HL));
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                _registers.A = (byte)(_registers.A ^ _bus.ReadMemory(_registers.PC));
                _registers.PC++;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
        _registers.SetFlag(Flag.Zero, _registers.A == 0);
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
        _registers.SetFlag(Flag.Carry, false);
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
                _registers.SetFlag(Flag.Zero, _registers.A - _registers.A == 0);
                _registers.SetFlag(Flag.Carry, _registers.A < _registers.A);
                _registers.SetHalfCarryFlagSubtracting(_registers.A, _registers.A);
                break;
            case InstructionParam.B:
                _registers.SetFlag(Flag.Zero, _registers.A - _registers.B == 0);
                _registers.SetFlag(Flag.Carry, _registers.A < _registers.B);
                _registers.SetHalfCarryFlagSubtracting(_registers.A, _registers.B);
                break;
            case InstructionParam.C:
                _registers.SetFlag(Flag.Zero, _registers.A - _registers.C == 0);
                _registers.SetFlag(Flag.Carry, _registers.A < _registers.C);
                _registers.SetHalfCarryFlagSubtracting(_registers.A, _registers.C);
                break;
            case InstructionParam.D:
                _registers.SetFlag(Flag.Zero, _registers.A - _registers.D == 0);
                _registers.SetFlag(Flag.Carry, _registers.A < _registers.D);
                _registers.SetHalfCarryFlagSubtracting(_registers.A, _registers.D);
                break;
            case InstructionParam.E:
                _registers.SetFlag(Flag.Zero, _registers.A - _registers.E == 0);
                _registers.SetHalfCarryFlagSubtracting(_registers.A, _registers.E);
                _registers.SetFlag(Flag.Carry, _registers.A < _registers.E);
                break;
            case InstructionParam.H:
                _registers.SetFlag(Flag.Zero, _registers.A - _registers.H == 0);
                _registers.SetHalfCarryFlagSubtracting(_registers.A, _registers.H);
                _registers.SetFlag(Flag.Carry, _registers.A < _registers.H);
                break;
            case InstructionParam.L:
                _registers.SetFlag(Flag.Zero, _registers.A - _registers.L == 0);
                _registers.SetHalfCarryFlagSubtracting(_registers.A, _registers.L);
                _registers.SetFlag(Flag.Carry, _registers.A < _registers.L);
                break;
            case InstructionParam.HLMem:
                _registers.SetFlag(Flag.Zero, _registers.A - _bus.ReadMemory(_registers.HL) == 0);
                _registers.SetHalfCarryFlagSubtracting(_registers.A, _bus.ReadMemory(_registers.HL));
                _registers.SetFlag(Flag.Carry, _registers.A < _bus.ReadMemory(_registers.HL));
                _cyclesLeft--;
                break;
            case InstructionParam.d8:
                _registers.SetFlag(Flag.Zero, _registers.A - _bus.ReadMemory(_registers.PC) == 0);
                _registers.SetHalfCarryFlagSubtracting(_registers.A, _bus.ReadMemory(_registers.PC));
                _registers.SetFlag(Flag.Carry, _registers.A < _bus.ReadMemory(_registers.PC));
                _registers.PC++;
                _cyclesLeft--;
                break;
            default:
                throw new NotSupportedException(param1.ToString());
        }
        
        _registers.SetFlag(Flag.Subtraction, true);

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
                _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
                _cyclesLeft--;

                _registers.SP -= 2;
                _registers.PC = 0x0000;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit1:
                _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
                _cyclesLeft--;

                _registers.SP -= 2;
                _registers.PC = 0x0008;
                _cyclesLeft--;
                break;

            case InstructionParam.Bit2:
                _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
                _cyclesLeft--;

                _registers.SP -= 2;
                _registers.PC = 0x0010;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit3:
                _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
                _cyclesLeft--;

                _registers.SP -= 2;
                _registers.PC = 0x0018;
                _cyclesLeft--;
                break;

            case InstructionParam.Bit4:
                _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
                _cyclesLeft--;

                _registers.SP -= 2;
                _registers.PC = 0x0020;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit5:
                _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
                _cyclesLeft--;

                _registers.SP -= 2;
                _registers.PC = 0x0028;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit6:
                _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
                _cyclesLeft--;

                _registers.SP -= 2;
                _registers.PC = 0x0030;
                _cyclesLeft--;
                break;
            case InstructionParam.Bit7:
                _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                _cyclesLeft--;

                _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
                _cyclesLeft--;

                _registers.SP -= 2;
                _registers.PC = 0x0038;
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
                _registers.SetFlag(Flag.Carry, (_registers.A & 1) == 1);
                _registers.A = (byte)(_registers.B >> 1);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                _registers.SetFlag(Flag.Carry, (_registers.B & 1) == 1);
                _registers.B = (byte)(_registers.B >> 1);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                break;
            case InstructionParam.C:
                _registers.SetFlag(Flag.Carry, (_registers.C & 1) == 1);
                _registers.C = (byte)(_registers.C >> 1);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                break;
            case InstructionParam.D:
                _registers.SetFlag(Flag.Carry, (_registers.D & 1) == 1);
                _registers.D = (byte)(_registers.D >> 1);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                break;
            case InstructionParam.E:
                _registers.SetFlag(Flag.Carry, (_registers.E & 1) == 1);
                _registers.E = (byte)(_registers.E >> 1);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                break;
            case InstructionParam.H:
                _registers.SetFlag(Flag.Carry, (_registers.H & 1) == 1);
                _registers.H = (byte)(_registers.H >> 1);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                break;
            case InstructionParam.L:
                _registers.SetFlag(Flag.Carry, (_registers.L & 1) == 1);
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                _registers.L = (byte)(_registers.L >> 1);
                break;
            case InstructionParam.HLMem:
                var memValue = _bus.ReadMemory(_registers.HL);
                _registers.SetFlag(Flag.Carry, (memValue & 1) == 1);
                var newValue = _registers.C >> 1;
                _bus.WriteMemory(_registers.HL, (byte)(newValue));
                _registers.SetFlag(Flag.Zero, newValue == 0);
                break;
            default:
                throw new InvalidOperationException(param1.ToString());
        }
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
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
                carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                _registers.SetFlag(Flag.Carry, (_registers.A & 1) == 1);
                _registers.A = (byte)((_registers.A >> 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                _registers.SetFlag(Flag.Carry, (_registers.B & 1) == 1);
                _registers.B = (byte)((_registers.B >> 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                break;
            case InstructionParam.C:
                carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                _registers.SetFlag(Flag.Carry, (_registers.C & 1) == 1);
                _registers.C = (byte)((_registers.C >> 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                break;
            case InstructionParam.D:
                carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                _registers.SetFlag(Flag.Carry, (_registers.D & 1) == 1);
                _registers.D = (byte)((_registers.D >> 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                break;
            case InstructionParam.E:
                carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                _registers.SetFlag(Flag.Carry, (_registers.E & 1) == 1);
                _registers.E = (byte)((_registers.E >> 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                break;
            case InstructionParam.H:
                carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                _registers.SetFlag(Flag.Carry, (_registers.H & 1) == 1);
                _registers.H = (byte)((_registers.H >> 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                break;
            case InstructionParam.L:
                carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);
                _registers.SetFlag(Flag.Carry, (_registers.L & 1) == 1);
                _registers.L = (byte)((_registers.L >> 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                break;
            case InstructionParam.HLMem:
                carryBit = (byte)((_registers.GetFlag(Flag.Carry) ? 1 : 0) << 7);

                var memoryValue = _bus.ReadMemory(_registers.HL);
                var newValue = (byte)((_registers.A >> 1) + carryBit);

                _registers.SetFlag(Flag.Carry, (memoryValue & 1) == 1);
                _bus.WriteMemory(_registers.HL, newValue);
                _registers.SetFlag(Flag.Zero, newValue == 0);
                break;
            default:
                throw new InvalidOperationException(param1.ToString());
        }

        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
    }

    /// <summary>
    /// Rotates the contents of param to the right
    /// </summary>
    /// <param name="param1"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void RL(InstructionParam param1)
    {
        var carryBit = (byte) (_registers.GetFlag(Flag.Carry) ? 1 : 0);
        switch (param1)
        {
            case InstructionParam.A:
                _registers.SetFlag(Flag.Carry, (_registers.A & 0b10000000) > 0);
                _registers.A = (byte)((_registers.A << 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                _registers.SetFlag(Flag.Carry, (_registers.B & 0b10000000) > 0);
                _registers.B = (byte)((_registers.B << 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                break;
            case InstructionParam.C:
                _registers.SetFlag(Flag.Carry, (_registers.C & 0b10000000) > 0);
                _registers.C = (byte)((_registers.C << 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                break;
            case InstructionParam.D:
                _registers.SetFlag(Flag.Carry, (_registers.D & 0b10000000) > 0);
                _registers.D = (byte)((_registers.D << 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                break;
            case InstructionParam.E:
                _registers.SetFlag(Flag.Carry, (_registers.E & 0b10000000) > 0);
                _registers.E = (byte)((_registers.E << 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                break;
            case InstructionParam.H:
                _registers.SetFlag(Flag.Carry, (_registers.H & 0b10000000) > 0);
                _registers.H = (byte)((_registers.H << 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                break;
            case InstructionParam.L:
                _registers.SetFlag(Flag.Carry, (_registers.L & 0b10000000) > 0);
                _registers.L = (byte)((_registers.L << 1) + carryBit);
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                break;
            case InstructionParam.HLMem:
                var memoryValue = _bus.ReadMemory(_registers.HL);
                var newValue = (byte)((_registers.A << 1) + carryBit);

                _registers.SetFlag(Flag.Carry, (memoryValue & 0b10000000) > 0);
                _bus.WriteMemory(_registers.HL, newValue);
                _registers.SetFlag(Flag.Zero, newValue == 0);
                break;
            default:
                throw new InvalidOperationException(param1.ToString());
        }
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
    }

    /// <summary>
    /// Adjusts the accumulator based on Subtraction and carry flags. Used if the previous instruction is a decimal calculation
    /// </summary>
    private void DAA()
    {
        if (_registers.GetFlag(Flag.Subtraction))
        {
            if (_registers.GetFlag(Flag.Carry)) { _registers.A -= 0x60; }
            if (_registers.GetFlag(Flag.HalfCarry)) { _registers.A -= 0x6; }
        }
        else
        {
            if (_registers.GetFlag(Flag.Carry) || _registers.A > 0x99) { _registers.A += 0x60; _registers.SetFlag(Flag.Carry, true); }
            if (_registers.GetFlag(Flag.HalfCarry) || (_registers.A & 0x0f) > 0x09) { _registers.A += 0x6; }
        }
        _registers.SetFlag(Flag.Zero, _registers.A == 0);
        _registers.SetFlag(Flag.HalfCarry, false);
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
                lowerNibble = _registers.A & 0x0F;
                upperNibble = (_registers.A & 0xF0) >> 4;

                _registers.A = (byte)((lowerNibble << 4) + upperNibble);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                lowerNibble = _registers.B & 0x0F;
                upperNibble = (_registers.B & 0xF0) >> 4;

                _registers.B = (byte)((lowerNibble << 4) + upperNibble);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                break;
            case InstructionParam.C:
                lowerNibble = _registers.C & 0x0F;
                upperNibble = (_registers.C & 0xF0) >> 4;

                _registers.C = (byte)((lowerNibble << 4) + upperNibble);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                break;
            case InstructionParam.D:
                lowerNibble = _registers.D & 0x0F;
                upperNibble = (_registers.D & 0xF0) >> 4;

                _registers.D = (byte)((lowerNibble << 4) + upperNibble);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                break;
            case InstructionParam.E:
                lowerNibble = _registers.E & 0x0F;
                upperNibble = (_registers.E & 0xF0) >> 4;

                _registers.E = (byte)((lowerNibble << 4) + upperNibble);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                break;
            case InstructionParam.H:
                lowerNibble = _registers.H & 0x0F;
                upperNibble = (_registers.H & 0xF0) >> 4;

                _registers.H = (byte)((lowerNibble << 4) + upperNibble);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                break;
            case InstructionParam.L:
                lowerNibble = _registers.L & 0x0F;
                upperNibble = (_registers.L & 0xF0) >> 4;

                _registers.L = (byte)((lowerNibble << 4) + upperNibble);
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                break;
            case InstructionParam.HLMem:
                var memVal = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                lowerNibble = memVal & 0x0F;
                upperNibble = (memVal & 0xF0) >> 4;

                _bus.WriteMemory(_registers.HL, (byte)((lowerNibble << 4) + upperNibble));
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                _cyclesLeft--;
                break;
            default: 
                throw new InvalidOperationException(param1.ToString());
        }
        _registers.SetFlag(Flag.HalfCarry, false);
        _registers.SetFlag(Flag.Carry, false);
        _registers.SetFlag(Flag.Subtraction, false);
    }

    private void HALT()
    {
        if (!_interrupts) {
            if ((_bus.ReadMemory((ushort)HardwareRegisters.IE) & _bus.ReadMemory((ushort)HardwareRegisters.IF) & 0x1F) == 0) {
                _halted = true;
                _registers.PC--;
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
                _registers.SetFlag(Flag.Carry, (_registers.A & 0b10000000) > 0);
                _registers.A = (byte)(_registers.A << 1);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                _registers.SetFlag(Flag.Carry, (_registers.B & 0b10000000) > 0);
                _registers.B = (byte)(_registers.B << 1);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                break;
            case InstructionParam.C:
                _registers.SetFlag(Flag.Carry, (_registers.C & 0b10000000) > 0);
                _registers.C = (byte)(_registers.C << 1);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                break;
            case InstructionParam.D:
                _registers.SetFlag(Flag.Carry, (_registers.D & 0b10000000) > 0);
                _registers.D = (byte)(_registers.D << 1);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                break;
            case InstructionParam.E:
                _registers.SetFlag(Flag.Carry, (_registers.E & 0b10000000) > 0);
                _registers.E = (byte)(_registers.E << 1);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                break;
            case InstructionParam.H:
                _registers.SetFlag(Flag.Carry, (_registers.H & 0b10000000) > 0);
                _registers.H = (byte)(_registers.H << 0);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                break;
            case InstructionParam.L:
                _registers.SetFlag(Flag.Carry, (_registers.L & 0b10000000) > 0);
                _registers.L = (byte)(_registers.L << 1);
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                break;
            case InstructionParam.HLMem:
                _registers.SetFlag(Flag.Carry, (_bus.ReadMemory(_registers.HL) & 0b10000000) > 0);
                _cyclesLeft--;
                _bus.WriteMemory((ushort) _registers.HL, (byte)(_bus.ReadMemory((_registers.HL)) << 1));
                _cyclesLeft--;
                _registers.SetFlag(Flag.Zero, _bus.ReadMemory(_registers.HL) == 0);
                break;
            default:
                throw new InvalidOperationException(param1.ToString());
        }
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, false);
    }
    
    private void SRA(InstructionParam param1)
        {
            switch (param1)
            {
                case InstructionParam.A:
                    _registers.SetFlag(Flag.Carry, (_registers.A & 1) > 0);
                    _registers.A = (byte)(_registers.A >> 1);
                    _registers.SetFlag(Flag.Zero, _registers.A == 0);
                    break;
                case InstructionParam.B:
                    _registers.SetFlag(Flag.Carry, (_registers.B & 1) > 0);
                    _registers.B = (byte)(_registers.B >> 1);
                    _registers.SetFlag(Flag.Zero, _registers.B == 0);
                    break;
                case InstructionParam.C:
                    _registers.SetFlag(Flag.Carry, (_registers.C & 1) > 0);
                    _registers.C = (byte)(_registers.C >> 1);
                    _registers.SetFlag(Flag.Zero, _registers.C == 0);
                    break;
                case InstructionParam.D:
                    _registers.SetFlag(Flag.Carry, (_registers.D & 1) > 0);
                    _registers.D = (byte)(_registers.D >> 1);
                    _registers.SetFlag(Flag.Zero, _registers.D == 0);
                    break;
                case InstructionParam.E:
                    _registers.SetFlag(Flag.Carry, (_registers.E & 1) > 0);
                    _registers.E = (byte)(_registers.E >> 1);
                    _registers.SetFlag(Flag.Zero, _registers.E == 0);
                    break;
                case InstructionParam.H:
                    _registers.SetFlag(Flag.Carry, (_registers.H & 1) > 0);
                    _registers.H = (byte)(_registers.H >> 0);
                    _registers.SetFlag(Flag.Zero, _registers.H == 0);
                    break;
                case InstructionParam.L:
                    _registers.SetFlag(Flag.Carry, (_registers.L & 1) > 0);
                    _registers.L = (byte)(_registers.L >> 1);
                    _registers.SetFlag(Flag.Zero, _registers.L == 0);
                    break;
                case InstructionParam.HLMem:
                    _registers.SetFlag(Flag.Carry, (_bus.ReadMemory(_registers.HL) & 1) > 0);
                    _cyclesLeft--;
                    _bus.WriteMemory( _registers.HL, (byte)(_bus.ReadMemory((_registers.HL)) >> 1));
                    _registers.SetFlag(Flag.Zero, _bus.ReadMemory(_registers.HL) == 0);
                    _cyclesLeft--;
                    break;
                default:
                    throw new InvalidOperationException(param1.ToString());
            }
            _registers.SetFlag(Flag.Subtraction, false);
            _registers.SetFlag(Flag.HalfCarry, false);
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
                _registers.A &= bitToReset;
                break;
            case InstructionParam.B:
                _registers.B &= bitToReset;
                break;
            case InstructionParam.C:
                _registers.C &= bitToReset;
                break;
            case InstructionParam.D:
                _registers.D &= bitToReset;
                break;
            case InstructionParam.E:
                _registers.E &= bitToReset;
                break;
            case InstructionParam.H:
                _registers.H &= bitToReset;
                break;
            case InstructionParam.L:
                _registers.L &= bitToReset;
                break;
            case InstructionParam.HLMem:
                var valueToUpdate = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                valueToUpdate  &= bitToReset;
                
                _bus.WriteMemory(_registers.HL, valueToUpdate);
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
                _registers.A |= bitToSet;
                break;
            case InstructionParam.B:
                _registers.B |= bitToSet;
                break;
            case InstructionParam.C:
                _registers.C |= bitToSet;
                break;
            case InstructionParam.D:
                _registers.D |= bitToSet;
                break;
            case InstructionParam.E:
                _registers.E |= bitToSet;
                break;
            case InstructionParam.H:
                _registers.H |= bitToSet;
                break;
            case InstructionParam.L:
                _registers.L |= bitToSet;
                break;
            case InstructionParam.HLMem:
                var valueToUpdate = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                valueToUpdate  |= bitToSet;
                
                _bus.WriteMemory(_registers.HL, valueToUpdate);
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
                bitSet = (_registers.A & bitToCopy) > 0;
                break;
            case InstructionParam.B:
                bitSet = (_registers.B & bitToCopy) > 0;
                break;
            case InstructionParam.C:
                bitSet = (_registers.C & bitToCopy) > 0;
                break;
            case InstructionParam.D:
                bitSet = (_registers.D & bitToCopy) > 0;
                break;
            case InstructionParam.E:
                bitSet = (_registers.E & bitToCopy) > 0;
                break;
            case InstructionParam.H:
                bitSet = (_registers.H & bitToCopy) > 0;
                break;
            case InstructionParam.L:
                bitSet = (_registers.L & bitToCopy) > 0;
                break;
            case InstructionParam.HLMem:
                var memoryValue = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                bitSet = (memoryValue & bitToCopy) > 0;
                
                break;
            default:
                throw new InvalidOperationException(param2.ToString());
        }
        _registers.SetFlag(Flag.Zero,  !bitSet );
        _registers.SetFlag(Flag.Subtraction, false);
        _registers.SetFlag(Flag.HalfCarry, true);
    }

    private bool? CheckCondition(InstructionParam condition)
    {
        bool? conditionMet;
        switch (condition)
        {
            case InstructionParam.NZ:
                conditionMet = !_registers.GetFlag(Flag.Zero);
                break;
            case InstructionParam.NC:
                conditionMet = !_registers.GetFlag(Flag.Carry);
                break;
            case InstructionParam.Z:
                conditionMet = _registers.GetFlag(Flag.Zero);
                break;
            case InstructionParam.C:
                conditionMet = _registers.GetFlag(Flag.Carry);
                break;
            default:
                // return null if not a condition
                conditionMet = null;
                break;
        }

        return conditionMet;
    }

}
