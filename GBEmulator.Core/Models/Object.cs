namespace GBEmulator.Core.Models;

/// <summary>
/// Represents the 4 bytes of an object stored in the OAM
/// </summary>
public class Object
{
    public int YPosition { get; set; }
    public int XPosition { get; set; }
    public byte TileIndex { get; set; }
    public byte Attributes { get; set; }
}