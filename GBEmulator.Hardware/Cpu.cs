namespace GBEmulator.Hardware;

using System;
using Core.Enums;
using Core.Interfaces;
using Core.Models;

public partial class Cpu : HardwareComponent, ICpu
{
    // Registers
    private readonly IRegisters _registers;
    private bool _interrupts = false;

    // Properties of current cycle
    private Instruction _currentInstruction;
    private byte _currentOpcode;
    private int _cyclesLeft;
    private bool _16BitOpcode;

    private readonly InstructionHelper _instructionHelper;
    private bool _halted;
    private bool _interruptsToBeEnabled;

    public Cpu(IRegisters? registers = null)
    {
        _instructionHelper = new InstructionHelper();
        _registers = registers ?? new Registers();
        _currentInstruction = null!;
        _bus = null!;
    }

    public int Clock(TextWriter? writer = null)
    {
        //if (_cyclesLeft != 0)
        //{
        //    throw new CycleError($"Expected {_currentInstruction.NumberOfCycles} cycles to be used. Actually used: {_currentInstruction.NumberOfCycles - _cyclesLeft} cycles.");
        //}

        if (_haltBug)
        {
            _registers.PC--;
            _haltBug = false;
        }

        _cyclesLeft = 0;

        writer ??= new StringWriter();
        if (_cyclesLeft == 0)
        {
            // Read the next opcode from memory
            _currentOpcode = _bus.ReadMemory(_registers.PC);

            // Debug ( Drastically  decreases performance )
            //LogStatus(writer);

            // Increment the program counter to point at the next byte of data
            _registers.PC++;

            // Get the instruction associated with the opcode
            _currentInstruction = GetInstruction(_currentOpcode);

            switch (_currentInstruction.Type)
            {
                case InstructionType.CALL:
                    if (!(CheckCondition(_currentInstruction.Param1) ?? true))
                    {
                        _currentInstruction.NumberOfCycles = 3;
                    }
                    break;
                case InstructionType.RET:
                    if (!(CheckCondition(_currentInstruction.Param1) ?? true))
                    {
                        _currentInstruction.NumberOfCycles = 2;
                    }
                    break;
                case InstructionType.JP:
                    if (!(CheckCondition(_currentInstruction.Param1) ?? true))
                    {
                        _currentInstruction.NumberOfCycles = 3;
                    }
                    break;
                case InstructionType.JR:
                    if (!(CheckCondition(_currentInstruction.Param1) ?? true))
                    {
                        _currentInstruction.NumberOfCycles = 2;
                    }
                    break;
            }

            // Update number of cycles to run instruction for
            _cyclesLeft = _currentInstruction.NumberOfCycles;
            _cyclesLeft -= _16BitOpcode ? 2 : 1;

            Execute();
        }

        return _currentInstruction.NumberOfCycles;
    }

    private void Execute()
    {
        if (_16BitOpcode)
        {
            Execute16BitOpCode();
        }
        else
        {
            Execute8BitOpCode();
        }
    }

    private void Execute8BitOpCode()
    {
        switch (_currentInstruction.Type)
        {
            case InstructionType.NOP:
                break;
            case InstructionType.LD:
                LD(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            case InstructionType.SCF:
                SCF();
                break;
            case InstructionType.INC:
                INC(_currentInstruction.Param1);
                break;
            case InstructionType.DEC:
                DEC(_currentInstruction.Param1);
                break;
            case InstructionType.ADD:
                ADD(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            case InstructionType.JP:
                JP(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            case InstructionType.JR:
                JR(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            case InstructionType.STOP:
                //throw new NotImplementedException();
                break;
            case InstructionType.PUSH:
                PUSH(_currentInstruction.Param1);
                break;
            case InstructionType.POP:
                POP(_currentInstruction.Param1);
                break;
            case InstructionType.ADC:
                ADC(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            case InstructionType.RLCA:
                RLCA();
                break;
            case InstructionType.RRCA:
                RRCA();
                break;
            case InstructionType.CPL:
                _registers.A = (byte)~_registers.A;
                _registers.SetFlag(Flag.HalfCarry, true);
                _registers.SetFlag(Flag.Subtraction, true);
                break;
            case InstructionType.CCF:
                _registers.SetFlag(Flag.HalfCarry, false);
                _registers.SetFlag(Flag.Subtraction, false);
                _registers.SetFlag(Flag.Carry, !_registers.GetFlag(Flag.Carry));
                break;
            case InstructionType.DI:
                _interrupts = false;
                break;
            case InstructionType.EI:
                // Interrupts get enabled after handling interrupts
                _interruptsToBeEnabled = true;
                break;
            case InstructionType.CALL:
                CALL(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            case InstructionType.RET:
                RET(_currentInstruction.Param1);
                break;
            case InstructionType.AND:
                AND(_currentInstruction.Param1);
                break;
            case InstructionType.OR:
                OR(_currentInstruction.Param1);
                break;
            case InstructionType.CP:
                CP(_currentInstruction.Param1);
                break;
            case InstructionType.RST:
                RST(_currentInstruction.Param1);
                break;
            case InstructionType.SUB:
                SUB(_currentInstruction.Param1);
                break;
            case InstructionType.XOR:
                XOR(_currentInstruction.Param1);
                break;
            case InstructionType.RRA:
                RRA();
                break;
            case InstructionType.RLA:
                RLA();
                break;
            case InstructionType.DAA:
                DAA();
                break;
            case InstructionType.SBC:
                SBC(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            case InstructionType.HALT:
                HALT();
                break;
            case InstructionType.RETI:
                RET(InstructionParam.NoParameter);
                _interrupts = true;
                break;
            default:
                throw new InvalidOperationException(_currentInstruction.Type.ToString());
        }
    }

    private void LogStatus(TextWriter writer)
    {
        string FormatShort(ushort s)
        {
            return s switch
            {
                > 0x00FF and <= 0x0FFF => "0" + Convert.ToString(s, 16),
                > 0x000F and <= 0x00FF => "00" + Convert.ToString(s, 16),
                > 0x0000 and <= 0x000F => "000" + Convert.ToString(s, 16),
                0 => "0000",
                _ => Convert.ToString(s, 16)
            };
        }

        string Format(byte b)
        {
            return b < 0x10 ? "0" + Convert.ToString(b, 16) : Convert.ToString(b, 16);
        }

        var a = Format(_registers.A);
        var f = Format(_registers.F);
        var b = Format(_registers.B);
        var c = Format(_registers.C);
        var d = Format(_registers.D);
        var e = Format(_registers.E);
        var h = Format(_registers.H);
        var l = Format(_registers.L);
        var pc = FormatShort(_registers.PC);
        var sp = FormatShort(_registers.SP);

        var line =
            $"A:{a} F:{f} B:{b} C:{c} D:{d} E:{e} H:{h} L:{l} SP:{sp} PC:{pc} PCMEM:{Format(_bus.ReadMemory(_registers.PC))},{Format(_bus.ReadMemory((ushort)(_registers.PC + 1)))},{Format(_bus.ReadMemory((ushort)(_registers.PC + 2)))},{Format(_bus.ReadMemory((ushort)(_registers.PC + 3)))}"
                .ToUpper();

        if (line == "A:04 F:10 B:01 C:00 D:C7 E:BA H:90 L:00 SP:DFFD PC:C2BE PCMEM:E0,0F,05,C2")
        {
            Console.Write("");
        }

        writer.WriteLine(line);
        //Console.WriteLine(line);
    }

    private void Execute16BitOpCode()
    {
        switch (_currentInstruction.Type)
        {
            case InstructionType.RLC:
                RLC(_currentInstruction.Param1);
                break;
            case InstructionType.RRC:
                RRC(_currentInstruction.Param1);
                break;
            case InstructionType.SRL:
                SRL(_currentInstruction.Param1);
                break;
            case InstructionType.RR:
                RR(_currentInstruction.Param1);
                break;
            case InstructionType.RL:
                RL(_currentInstruction.Param1);
                break;
            case InstructionType.SWAP:
                SWAP(_currentInstruction.Param1);
                break;
            case InstructionType.SLA:
                SLA(_currentInstruction.Param1);
                break;
            case InstructionType.SRA:
                SRA(_currentInstruction.Param1);
                break;
            case InstructionType.RES:
                RES(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            case InstructionType.SET:
                SET(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            case InstructionType.BIT:
                BIT(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
            default:
                throw new InvalidOperationException(_currentInstruction.Type.ToString());
        }
    }

    public void Reset(bool skipBoot)
    {
        _registers.A = 0x01;
        _registers.F = 0xB0;
        _registers.B = 0x00;
        _registers.C = 0x13;
        _registers.D = 0x00;
        _registers.E = 0xD8;
        _registers.H = 0x01;
        _registers.L = 0x4D;
        _registers.SP = 0xFFFE;
        _registers.PC = (ushort)(skipBoot ? 0x0100 : 0x0000);
    }

    public void Interrupt(Interrupt requestedInterrupt)
    {
        var interruptFlags = _bus.ReadMemory((ushort)HardwareRegisters.IF);
        var interruptSet = interruptFlags | (1 << (int)requestedInterrupt);

        _bus.WriteMemory((ushort)HardwareRegisters.IF, (byte)interruptSet);
    }

    /// <summary>
    /// Get instruction by opcode using <see cref="InstructionHelper.Lookup"/>
    /// </summary>
    /// <param name="opcode"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private Instruction GetInstruction(byte opcode)
    {
        Instruction fetchedInstruction;
        // 16-bit opcode prefixed with 0xCB
        if (opcode == 0xCB)
        {
            opcode = _bus.ReadMemory(_registers.PC);
            _registers.PC++;
            _16BitOpcode = true;
            fetchedInstruction = _instructionHelper.Lookup16bit[opcode].FirstOrDefault() ??
                                 throw new NotSupportedException(opcode.ToString());
        }
        else
        {
            _16BitOpcode = false;
            fetchedInstruction = _instructionHelper.Lookup[opcode].FirstOrDefault() ??
                                 throw new NotSupportedException(opcode.ToString());
        }

        return fetchedInstruction;
    }

    private void ExecuteInterrupt(Interrupt interruptType)
    {
        if (_halted)
        {
            _registers.PC++;
            _halted = false;
        }

        if (_interrupts)
        {
            PUSH(InstructionParam.PC);
            _registers.PC = (ushort)(0x40 + (8 * (int)interruptType));
            _interrupts = false;
            var requestedFlags = _bus.ReadMemory((ushort)HardwareRegisters.IF);
            requestedFlags &= (byte)(~(1 << (int)interruptType));
            
            _bus.WriteMemory((ushort)HardwareRegisters.IF, requestedFlags);
        }
    }

    public void HandleInterrupts()
    {
        var enabledInterrupts = _bus.ReadMemory((ushort)HardwareRegisters.IE);
        var requestedInterrupts = _bus.ReadMemory((ushort)HardwareRegisters.IF);

        for (var i = 0; i < 5; i++)
        {
            if ((((enabledInterrupts & requestedInterrupts) >> i) & 1) == 1)
            {
                ExecuteInterrupt((Interrupt)i);
            }
        }

        _interrupts |= _interruptsToBeEnabled;
        _interruptsToBeEnabled = false;
    }
}

internal class CycleError : Exception
{
    public CycleError(string message) : base(message)
    {
    }
}