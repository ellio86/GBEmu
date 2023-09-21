namespace GBEmulator.App;
using System.Diagnostics;
using System;
using Core.Enums;
using Core.Interfaces;
using Core.Models;

public partial class Cpu : ICpu
{
    // Registers
    private readonly IRegisters _registers;
    private bool _interupts = false;

    // Properties of current cycle
    private Instruction _currentInstruction;
    private byte _currentOpcode;
    private int _cyclesLeft;
    private bool _16bitOpcode;

    // Clock controls
    private bool _clockRunning = false;
    private Stopwatch _stopwatch;

    // Bus that CPU is connected to
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

    public void Clock(TextWriter? writer = null)
    {
        if (_cyclesLeft != 0)
        {
            _cyclesLeft = 0;
        }

        writer ??= new StringWriter();
        if (_cyclesLeft == 0)
        {
            // Read the next opcode from memory
            _currentOpcode = _bus.ReadMemory(_registers.PC);

            // Debug
            LogStatus(writer);

            // Increment the program counter to point at the next byte of data
            _registers.PC++;

            // Get the instruction associated with the opcode
            _currentInstruction = GetInstruction(_currentOpcode);

            // Update number of cycles to run instruction for
            _cyclesLeft = _currentInstruction.NumberOfCycles;

            _cyclesLeft -= _16bitOpcode ? 2 : 1;

            Execute();
        }
    }

    private void Execute()
    {
        if (_16bitOpcode)
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
                StopClock();
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
                break;
            case InstructionType.CCF:
                _registers.SetFlag(Flag.Carry, !_registers.GetFlag(Flag.Carry));
                break;
            case InstructionType.DI:
                _interupts = false;
                break;
            case InstructionType.EI:
                _interupts = true;
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
            default:
                throw new InvalidOperationException(_currentInstruction.Type.ToString());
        }
    }

    private void LogStatus(TextWriter writer)
    {
        string Format(byte s)
        {
            return s >= 0x10 ? Convert.ToString(s, 16) : "0" + Convert.ToString(s, 16);
        }

        var a = Format(_registers.A);
        var f = Format(_registers.F);
        var b = Format(_registers.B);
        var c = Format(_registers.C);
        var d = Format(_registers.D);
        var e = Format(_registers.E);
        var h = Format(_registers.H);
        var l = Format(_registers.L);

        var pc = _registers.PC >= 0x1000
            ? Convert.ToString(_registers.PC, 16)
            : "0" + Convert.ToString(_registers.PC, 16);
        var line = $"A: {a} F: {f} B: {b} C: {c} D: {d} E: {e} H: {h} L: {l} SP: {Convert.ToString(_registers.SP, 16)} PC: 00:{pc} ({Format(_bus.ReadMemory(_registers.PC))} {Format(_bus.ReadMemory((ushort)(_registers.PC + 1)))} {Format(_bus.ReadMemory((ushort)(_registers.PC + 2)))} {Format(_bus.ReadMemory((ushort)(_registers.PC + 3)))})".ToUpper();

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
            default:
                throw new InvalidOperationException(_currentInstruction.Type.ToString());
        }
    }

    private void StopClock()
    {
        _clockRunning = false;
    }

    public void Reset()
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
        _registers.PC = 0x0100;
        _bus.Reset();
        if(_bus.CartridgeLoaded) _bus.ReadRom();
        StartClock();
    }

    public void StartClock()
    {
        _stopwatch = Stopwatch.StartNew();
        _clockRunning = true;
        using var writer = new StreamWriter(@"..\..\..\log.txt");
        while (_clockRunning)
        {
            // pause for 0.25 milliseconds to simulate 4KHz (4000 times a second)
            //if (_stopwatch.ElapsedMilliseconds < 0.25)
            //{
            //    continue;
            //}
            //_stopwatch = Stopwatch.StartNew();

            // Tick the clock
            Clock(writer);

            // Listen to serial io port for test results
            if (_bus.ReadMemory(0xff02) == 0x81)
            {
                var c = (char)_bus.ReadMemory(0xff01);
                Console.Write(c);
                _bus.WriteMemory(0xff02, 0x00);
            }
        }
    }

    /// <summary>
    /// Get instruction by opcode using <see cref="InstructionHelper.Lookup"/>
    /// </summary>
    /// <param name="opcode"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private Instruction GetInstruction(byte opcode)
    {
        // 16-bit opcode prefixed with 0xCB
        if (opcode == 0xCB)
        {
            opcode = _bus.ReadMemory(_registers.PC);
            _registers.PC++;
            _16bitOpcode = true;
            return InstructionHelper.Lookup16bit[opcode].FirstOrDefault() ?? throw new NotSupportedException(opcode.ToString());
        }

        _16bitOpcode = false;
        return InstructionHelper.Lookup[opcode].FirstOrDefault() ?? throw new NotSupportedException(opcode.ToString());
    }
}
