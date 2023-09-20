﻿namespace GBEmulator.App;

using System;
using Core.Enums;
using Core.Interfaces;
using Core.Models;

public class Cpu : ICpu
{
    private readonly IRegisters _registers;
    private Instruction _currentInstruction;
    private byte _currentOpcode;
    private int _cyclesLeft;
    private IBus _bus;

    public Cpu(IRegisters registers)
    {
        _registers = registers ?? throw new ArgumentNullException(nameof(registers));
        _currentInstruction = null!;
        _bus = null!;
    }

    public void ConnectToBus(IBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }

    public void Clock()
    {
        if (_cyclesLeft == 0)
        {
            // Read the next opcode from memory
            _currentOpcode = _bus.ReadMemory(_registers.PC);

            // Increment the program counter to point at the next byte of data
            _registers.PC++;

            // Get the instruction associated with the opcode
            _currentInstruction = GetInstruction(_currentOpcode);

            // Update number of cycles to run instruction for
            _cyclesLeft = _currentInstruction.NumberOfCycles;

            Execute();
        }
    }

    private void Execute()
    {
        switch (_currentInstruction.Type)
        {
            case InstructionType.NOP:
                _registers.PC++;
                _cyclesLeft--;
                break;
            case InstructionType.LD:
                LD(_currentInstruction.Param1, _currentInstruction.Param2);
                _cyclesLeft--;
                break;
            case InstructionType.SCF:
                _registers.SetFlag(Flag.HalfCarry, false);
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Carry, true);
                _cyclesLeft--;
                break;
            case InstructionType.INC:
                INC(_currentInstruction.Param1);
                _cyclesLeft--;
                break;
            case InstructionType.DEC:
                DEC(_currentInstruction.Param1);
                _cyclesLeft--;
                break;
            case InstructionType.ADD:
                ADD(_currentInstruction.Param1, _currentInstruction.Param2);
                _cyclesLeft--;
                break;
            default:
                throw new InvalidOperationException(_currentInstruction.Type.ToString());
        }
    }

    public void Reset()
    {
        _registers.AF = _registers.BC = _registers.DE = _registers.HL = 0x0000;
        _bus.Reset();

    }

    /// <summary>
    /// Get instruction by opcode using <see cref="InstructionHelper.Lookup"/>
    /// </summary>
    /// <param name="opcode"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private Instruction GetInstruction(byte opcode)
    {
        return InstructionHelper.Lookup[opcode].FirstOrDefault() ?? throw new NotSupportedException(opcode.ToString());
    }

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
                addressToRead = (ushort) (0xFF00 + _registers.C);
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
                _registers.A = (byte) (valueToAdd + _registers.A);
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
}
