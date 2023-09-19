namespace GBEmulator.Core.Enums;

/// <summary>
/// Enum representing all of the possible instruction parameters
/// </summary>
public enum InstructionParam
{
    // Registers
    A,
    B,
    C,
    D,
    E,
    H,
    L,
    SP,
    PC,

    // Virtual 16-bit Registers
    BC,
    DE,
    HL,

    // Memory locations
    a16Mem,
    BCMem,
    DEMem,
    HLMem,

    // Memory location AND incrementation/decrementaion of virtual register
    HLMemInc,
    HLMemDec,

    // Actual values
    d8,
    d16,

    // Empty param
    NoParameter
}