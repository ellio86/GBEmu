        namespace GBEmulator.App;
using System.Diagnostics;
using System;
using Core.Enums;
using Core.Interfaces;
using Core.Models;

public partial class Cpu : ICpu
{
    private readonly IRegisters _registers;
    private Instruction _currentInstruction;
    private byte _currentOpcode;
    private int _cyclesLeft;
    private bool _clockRunning = false;
    private IBus _bus;
    private Stopwatch _stopwatch;

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
            case InstructionType.JR:
                JR(_currentInstruction.Param1, _currentInstruction.Param2);
                _cyclesLeft--;
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
        _registers.AF = _registers.BC = _registers.DE = _registers.HL = 0x0000;
        _registers.PC = 0x0150;
        _bus.Reset();
        if(_bus.CartridgeLoaded) _bus.ReadRom();
        StartClock();

    }

    public void StartClock()
    {
        _stopwatch = Stopwatch.StartNew();
        _clockRunning = true;
        while (_clockRunning)
        {
            // pause for 0.25 milliseconds to simulate 4KHz (4000 times a second)
            if (_stopwatch.ElapsedMilliseconds < 0.25)
            {
                continue;
            }
            _stopwatch = Stopwatch.StartNew();

            // Tick the clock
            Clock();

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
        return InstructionHelper.Lookup[opcode].FirstOrDefault() ?? throw new NotSupportedException(opcode.ToString());
    }
}
