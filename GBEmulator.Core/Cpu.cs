namespace GBEmulator.Core;
using System;

public class Cpu : ICpu
{
    public IRegisters _registers;

    private Instruction _currentInstruction;
    private byte _currentOpcode = 0x00;
    private int _cyclesLeft = 0;
    private Bus _bus;

    public Cpu(IRegisters registers)
    {
        _registers = registers ?? throw new ArgumentNullException(nameof(registers));
    }

    public void ConnectToBus(Bus bus)
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

    #region Instruction Mappings

    private void Execute()
    {
        switch (_currentInstruction.Type)
        {
            case InstructionType.LD:
                LD(_currentInstruction.Param1, _currentInstruction.Param2);
                break;
        }
    }

    #endregion

    public void Reset()
    {
        throw new NotImplementedException();
    }

    private Instruction GetInstruction(byte opcode)
    {
        return InstructionHelper.Lookup[opcode].FirstOrDefault() ?? throw new NotImplementedException(opcode.ToString());
    }

    /// <summary>
    /// Load Instruction
    /// </summary>
    private void LD(InstructionParam loadTo, InstructionParam dataToLoad)
    {
        byte data = 0x00;
        byte extraData = 0x00;
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
            case InstructionParam.HLMemInc:
                data = _bus.ReadMemory(_registers.HL);
                _registers.HL++;
                _cyclesLeft--;
                break;
            case InstructionParam.HLMemDec:
                data = _bus.ReadMemory(_registers.HL);
                _registers.HL--;
                _cyclesLeft--;
                break;
            default:
                throw new InvalidOperationException(nameof(dataToLoad));
        }

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
            case InstructionParam.HLMemInc:
                _bus.WriteMemory(_registers.HL, data);
                _registers.HL++;
                _cyclesLeft--;
                break;
            case InstructionParam.HLMemDec:
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
                _registers.SP = (ushort)((extraData << 8) & data);
                break;
            default:
                throw new InvalidOperationException(loadTo.ToString());
        }

        _cyclesLeft--;
    }
}
