namespace GBEmulator.Core;

/// <summary>
/// Enum representing all of the possible instruction parameters
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
    CP
}