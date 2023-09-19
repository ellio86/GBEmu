namespace GBEmulator.Core;
public class Instruction
{
    public required InstructionType Type;
    public required byte Opcode;
    public required int NumberOfBytes;
    public required int NumberOfCycles;
    public InstructionParam Param1 = InstructionParam.NoParameter;
    public InstructionParam Param2 = InstructionParam.NoParameter;
    public bool? ZeroFlag = null;
    public bool? SubtractFlag = null;
    public bool? HalfCarryFlag = null;
    public bool? CarryFlag = null;
}
