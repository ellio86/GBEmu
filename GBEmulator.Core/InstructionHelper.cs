namespace GBEmulator.Core;

public class InstructionHelper
{
    private static readonly Instruction[] Instructions = {
        new() { Type = InstructionType.NOP, Opcode = 0x00, NumberOfBytes = 1, NumberOfCycles = 1},
        new() { Type = InstructionType.LD, Opcode = 0x01, NumberOfBytes = 3, NumberOfCycles = 3, Param1 = InstructionParam.BC, Param2 = InstructionParam.d16},
        new() { Type = InstructionType.LD, Opcode = 0x02, NumberOfBytes = 1, NumberOfCycles = 2, Param1 = InstructionParam.BCMem, Param2 = InstructionParam.A},
        new() { Type = InstructionType.INC, Opcode = 0x03, NumberOfBytes = 1, NumberOfCycles = 2, Param1 = InstructionParam.BC},
        new() { Type = InstructionType.INC, Opcode = 0x04, NumberOfBytes = 1, NumberOfCycles = 1, Param1 = InstructionParam.B},
        new() { Type = InstructionType.DEC, Opcode = 0x05, NumberOfBytes = 1, NumberOfCycles = 1, Param1 = InstructionParam.B},
        new() { Type = InstructionType.LD, Opcode = 0x06, NumberOfBytes = 2, NumberOfCycles = 2, Param1 = InstructionParam.B, Param2 = InstructionParam.d8},
        new() { Type = InstructionType.RLCA, Opcode = 0x07, NumberOfBytes = 1, NumberOfCycles = 1},
        new() { Type = InstructionType.LD, Opcode = 0x08, NumberOfBytes = 3, NumberOfCycles = 5, Param1 = InstructionParam.a16Mem, Param2 = InstructionParam.SP},
        new() { Type = InstructionType.ADD, Opcode = 0x09, NumberOfBytes = 1, NumberOfCycles = 2, Param1 = InstructionParam.HL, Param2 = InstructionParam.BC},
        new() { Type = InstructionType.ADD, Opcode = 0x0A, NumberOfBytes = 1, NumberOfCycles = 2, Param1 = InstructionParam.A, Param2 = InstructionParam.BCMem},
        new() { Type = InstructionType.DEC, Opcode = 0x0B, NumberOfBytes = 1, NumberOfCycles = 2, Param1 = InstructionParam.BC},
        new() { Type = InstructionType.INC, Opcode = 0x0C, NumberOfBytes = 1, NumberOfCycles = 1, Param1 = InstructionParam.C},
        new() { Type = InstructionType.DEC, Opcode = 0x0D, NumberOfBytes = 1, NumberOfCycles = 1, Param1 = InstructionParam.C},
        new() { Type = InstructionType.LD, Opcode = 0x0E, NumberOfBytes = 2, NumberOfCycles = 2, Param1 = InstructionParam.C, Param2 = InstructionParam.d8},
        new() { Type = InstructionType.RRCA, Opcode = 0x0F, NumberOfBytes = 1, NumberOfCycles = 1}
    };

    public static Lookup<byte, Instruction> Lookup => (Lookup<byte, Instruction>)Instructions.ToLookup(instruction => instruction.Opcode, instruction => instruction);
}

public static class InstructionParamHelperExtensions
{
    private static IEnumerable<InstructionParam> _registers = new List<InstructionParam>
    {
        InstructionParam.A, InstructionParam.B, InstructionParam.C, InstructionParam.D,
        InstructionParam.E, InstructionParam.H, InstructionParam.L, InstructionParam.SP,
        InstructionParam.PC, InstructionParam.BC, InstructionParam.DE, InstructionParam.HL
    };

    private static IEnumerable<InstructionParam> _memoryLocations = new List<InstructionParam>
    {
        InstructionParam.a16Mem, InstructionParam.BCMem, InstructionParam.DEMem, InstructionParam.HLMem,
        InstructionParam.HLMemInc, InstructionParam.HLMemDec,
    };

    private static IEnumerable<InstructionParam> _values = new List<InstructionParam>
    {
        InstructionParam.d8, InstructionParam.d16
    };

    public static bool IsRegister(this InstructionParam instructionParam)
    {
        return _registers.Contains(instructionParam);
    }

    public static bool IsMemoryLocation(this InstructionParam instructionParam)
    {
        return _memoryLocations.Contains(instructionParam);
    }

    public static bool IsValue(this InstructionParam instructionParam)
    {
        return _values.Contains(instructionParam);
    }
}