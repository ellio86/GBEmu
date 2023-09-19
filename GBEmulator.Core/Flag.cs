namespace GBEmulator.Core;

/// <summary>
/// Represents the top 4 bits of the flag register. Subtraction and half carry flags are used specifically for adding/subtracting decimals (BCD numbers)
/// </summary>
public enum Flag {
    /// <summary>
    /// 7th (most significant) bit. Zero flag is set when the result of an operation is 0
    /// </summary>
    Zero = (1 << 7),

    /// <summary>
    /// 6th bit. Indicates whether the previous instruction was a subtraction. Used by DAA instruction only. 
    /// </summary>
    Subtraction = (1 << 6),

    /// <summary>
    /// 5th bit. Indicates carry for the lower 4 bits of DAA result. Used by DAA instruction only. DAA also uses Carry flag for the upper 4 bits of the result. 
    /// </summary>
    HalfCarry = (1 << 5),

    /// <summary>
    /// 4th bit. Carry flag is set when:
    /// <list type="bullet">
    /// <item>The result of an 8-bit addition is higher than 0xFF</item>
    /// <item>The result of an 16-bit addition is higher than 0xFFFF</item>
    /// <item>The result of a subtraction or comparison is less than 0</item>
    /// <item>The result of a rotate/shift shifts out a "1" bit</item>
    /// </list>
    /// </summary>
    Carry = (1 << 4),
}
