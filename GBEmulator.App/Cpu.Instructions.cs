using System.ComponentModel;
using System.Xml.Xsl;

namespace GBEmulator.App;

using Core.Enums;
using System;

public partial class Cpu
{
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
                data = _registers.H;
                extraData = _registers.L;
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
                addressToRead = (ushort)(_bus.ReadMemory(_registers.PC) << 8);
                _registers.PC++;
                _cyclesLeft--;

                addressToRead += _bus.ReadMemory(_registers.PC);
                _registers.PC++;
                _cyclesLeft--;

                data = _bus.ReadMemory(addressToRead);
                _cyclesLeft--;
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
                addressToWrite = (ushort)(_bus.ReadMemory(_registers.PC) << 8);
                _registers.PC++;
                _cyclesLeft--;
                addressToWrite += _bus.ReadMemory(_registers.PC);
                _registers.PC++;
                _cyclesLeft--;

                if (dataToLoad is InstructionParam.SP)
                {

                    _bus.WriteMemory(addressToWrite, data);
                    _cyclesLeft--;
                    addressToWrite++;

                    _bus.WriteMemory(addressToWrite, extraData);
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
        ushort valueToAdd;
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
            default:
                throw new NotSupportedException(paramToAdd.ToString());
        }

        switch (paramToAddTo)
        {
            case InstructionParam.A:
                _registers.SetCarryFlags(_registers.A, (byte)valueToAdd);
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.A = (byte)(valueToAdd + _registers.A);
                _registers.SetFlag(Flag.Zero, _registers.A == 0x00);

                break;
            case InstructionParam.HL:
                _registers.SetCarryFlags(_registers.HL, valueToAdd);
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.HL += valueToAdd;
                _cyclesLeft--;
                break;
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
                _registers.SetCarryFlags(_registers.A, (byte)(carryValue + valueToAdd));
                _registers.A += (byte) (carryValue + valueToAdd);
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
                _registers.BC++;
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
                _registers.SetHalfCarryFlagSubtracting(_registers.A, -1);
                _registers.A--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                break;
            case InstructionParam.B:
                _registers.SetHalfCarryFlagSubtracting(_registers.B, -1);
                _registers.B--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                break;
            case InstructionParam.C:
                _registers.SetHalfCarryFlagSubtracting(_registers.C, -1);
                _registers.C--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                break;
            case InstructionParam.D:
                _registers.SetHalfCarryFlagSubtracting(_registers.D, -1);
                _registers.D--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                break;
            case InstructionParam.E:
                _registers.SetHalfCarryFlagSubtracting(_registers.E, -1);
                _registers.E--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                break;
            case InstructionParam.H:
                _registers.SetHalfCarryFlagSubtracting(_registers.H, -1);
                _registers.H--;
                _registers.SetFlag(Flag.Subtraction, true);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                break;
            case InstructionParam.L:
                _registers.SetHalfCarryFlagSubtracting(_registers.L, -1);
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
                _registers.BC--;
                _cyclesLeft--;
                break;
            case InstructionParam.HLMem:
                var valueToDecrement = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                _registers.SetHalfCarryFlagSubtracting(valueToDecrement, -1);
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
            _cyclesLeft--;
            return;
        }

        switch (param2)
        {
            case InstructionParam.s8:
                if ((bool)conditionMet)
                {
                    Jump((sbyte)_bus.ReadMemory(_registers.PC));
                    _cyclesLeft--;
                }
                else
                {
                    _cyclesLeft--;
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
                _registers.F = _bus.ReadMemory(_registers.SP);
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
    /// Moves program counter to param 2 and adds current program counter to the stack if condtion param1 is met
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
                    _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)((_registers.PC & 0xFF00) >> 8));
                    _cyclesLeft--;

                    _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
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
                    _bus.WriteMemory((ushort)(_registers.SP - 1), (byte)(_registers.PC & 0xFF00 >> 8));
                    _cyclesLeft--;

                    _bus.WriteMemory((ushort)(_registers.SP - 2), (byte)(_registers.PC & 0x00FF));
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
                bit7 = (byte)((_registers.A & 0b100000000) >> 8);
                _registers.A = (byte)((_registers.A << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.B:
                bit7 = (byte)((_registers.B & 0b100000000) >> 8);
                _registers.B = (byte)((_registers.B << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.C:
                bit7 = (byte)((_registers.C & 0b100000000) >> 8);
                _registers.C = (byte)((_registers.C << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.D:
                bit7 = (byte)((_registers.D & 0b100000000) >> 8);
                _registers.D = (byte)((_registers.D << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.E:
                bit7 = (byte)((_registers.E & 0b100000000) >> 8);
                _registers.E = (byte)((_registers.E << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.H:
                bit7 = (byte)((_registers.H & 0b100000000) >> 8);
                _registers.H = (byte)((_registers.H << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.L:
                bit7 = (byte)((_registers.L & 0b100000000) >> 8);
                _registers.L = (byte)((_registers.L << 1) + bit7);
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                break;
            case InstructionParam.HLMem:
                var val = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                bit7 = (byte)((val & 0b100000000) >> 8);
                val = (byte)((val << 1) + bit7);
                _bus.WriteMemory(_registers.HL, val);
                _registers.SetFlag(Flag.Zero, val == 0);
                _registers.SetFlag(Flag.Carry, bit7 == 1);
                _cyclesLeft--;
                break;
        }
    }    
    
    /// <summary>
    /// Perform right shift on param, setting the carry bit as necessary
    /// </summary>
    /// <param name="param1"></param>
    private void RRC(InstructionParam param1)
    {
        byte bit0;
        switch (param1)
        {
            case InstructionParam.A:
                bit0 = (byte)(_registers.A & 0b1);
                _registers.A = (byte)((_registers.A >> 1) + (bit0 << 8));
                _registers.SetFlag(Flag.Zero, _registers.A == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.B:
                bit0 = (byte)(_registers.B & 0b1);
                _registers.B = (byte)((_registers.B >> 1) + (bit0 << 8));
                _registers.SetFlag(Flag.Zero, _registers.B == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.C:
                bit0 = (byte)(_registers.C & 0b1);
                _registers.C = (byte)((_registers.C >> 1) + (bit0 << 8));
                _registers.SetFlag(Flag.Zero, _registers.C == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.D:
                bit0 = (byte)(_registers.D & 0b1);
                _registers.D = (byte)((_registers.D >> 1) + (bit0 << 8));
                _registers.SetFlag(Flag.Zero, _registers.D == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.E:
                bit0 = (byte)(_registers.E & 0b1);
                _registers.E = (byte)((_registers.E >> 1) + (bit0 << 8));
                _registers.SetFlag(Flag.Zero, _registers.E == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.H:
                bit0 = (byte)(_registers.H & 0b1);
                _registers.H = (byte)((_registers.H >> 1) + (bit0 << 8));
                _registers.SetFlag(Flag.Zero, _registers.H == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.L:
                bit0 = (byte)((_registers.L & 0b100000000) >> 8);
                _registers.L = (byte)((_registers.L >> 1) + (bit0 << 8));
                _registers.SetFlag(Flag.Zero, _registers.L == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                break;
            case InstructionParam.HLMem:
                var val = _bus.ReadMemory(_registers.HL);
                _cyclesLeft--;
                bit0 = (byte)(val & 0b1);
                val = (byte)((val >> 1) + (bit0 << 8));
                _bus.WriteMemory(_registers.HL, val);
                _registers.SetFlag(Flag.Zero, val == 0);
                _registers.SetFlag(Flag.Carry, bit0 == 1);
                _cyclesLeft--;
                break;
        }
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
            default:
                throw new NotSupportedException(param1.ToString());
        }
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
