using GBEmulator.Core.Enums;

namespace GBEmulator.Core.Interfaces;

public interface IRegisters
{
    /// <summary>
    /// Accumulator Register. Higher half of virtual 16 bit AF Register.
    /// </summary>
    public byte A { get; set; }

    /// <summary>
    /// B register. Used as part of virtual 16 bit BC register.
    /// </summary>
    public byte B { get; set; }

    /// <summary>
    /// C register. Used as part of virtual 16 bit BC register.
    /// </summary>
    public byte C { get; set; }

    /// <summary>
    /// D register. Used as part of virtual 16 bit DE register.
    /// </summary>
    public byte D { get; set; }


    /// <summary>
    /// E register. Used as part of virtual 16 bit DE register.
    /// </summary>
    public byte E { get; set; }

    /// <summary>
    /// Flags register. Lower half of virtual 16 bit AF Register.
    /// </summary>
    public byte F { get; set; }

    /// <summary>
    /// H register. Used as part of virtual 16 bit HL register.
    /// </summary>
    public byte H { get; set; }

    /// <summary>
    /// L register. Used as part of virtual 16 bit HL register.
    /// </summary>
    public byte L { get; set; }


    /// <summary>
    /// Virtual AF Register made from Accumulator and Flag registers.
    /// </summary>
    public ushort AF { get; set; }

    ///<summary>
    /// Virtual BC Register made from B and C registers.
    /// </summary>
    public ushort BC { get; set; }

    ///<summary>
    /// Virtual DE Register made from D and E registers.
    /// </summary>
    public ushort DE { get; set; }

    ///<summary>
    /// Virtual HL Register made from H and L registers.
    /// </summary>
    public ushort HL { get; set; }

    ///<summary>
    /// Stack Pointer
    /// </summary>
    public ushort SP { get; set; }

    ///<summary>
    /// Program Counter
    /// </summary>
    public ushort PC { get; set; }

    /// <summary>
    /// Gets value of flag from flag register
    /// </summary>
    /// <param name="flag">Flag to get value of</param>
    /// <returns></returns>
    public bool GetFlag(Flag flag);

    /// <summary>
    /// Set the value of a flag in the flag register
    /// </summary>
    /// <param name="flag">Flag to set</param>
    /// <param name="value">Value to set flag to</param>
    public void SetFlag(Flag flag, bool value);
}