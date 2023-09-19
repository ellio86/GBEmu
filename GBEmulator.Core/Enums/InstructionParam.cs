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
    AF,

    // Memory locations
    a16Mem,
    BCMem,
    DEMem,
    HLMem,
    
    // One byte memory location between 0x
    CMem,

    // Memory location AND incrementation/decrementaion of virtual register
    HLMemInc,
    HLMemDec,

    // Actual values
    d8,
    d16,

    // Empty param
    NoParameter,

    // Param specifies a certain bit
    Bit,

    // TODO: Figure these out
    s8,
    NZ,
    Z,
    NC
}