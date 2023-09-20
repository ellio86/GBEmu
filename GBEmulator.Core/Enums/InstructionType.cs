namespace GBEmulator.Core.Enums;

/// <summary>
/// Enum representing all of the types of instruction. This enum is derived from each instruction's mnemonic
/// </summary>
public enum InstructionType
{
    NOP,
    STOP,
    JR,
    LD,
    INC,
    DEC,
    RLCA,
    RLA,
    DAA,
    SCF,
    ADD,
    RRCA,
    RRA,
    CPL,
    CCF,
    HALT,
    RET,
    POP,
    JP,
    CALL,
    PUSH,
    DI,
    RST,
    RETI,
    EI,
    ADC,
    SBC,
    XOR,
    CP,
    AND,
    OR,
    SUB,
    RLC,
    RRC
}