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
    
    // One byte memory location between 0xFF00 - 0xFFFF;
    CMem,
    a8Mem,

    // Memory location AND incrementation/decrementaion of virtual register
    HLIMem,
    HLDMem,

    // Actual values
    d8,
    d16,

    // Empty param
    NoParameter,

    // Param specifies a certain bit
    Bit,

    // Signed byte
    s8,

    // Conditions
    // If Z flag is 0
    NZ,

    // If Z flag is 1
    Z,

    // If Carry flag is 0
    NC
}