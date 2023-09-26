namespace GBEmulator.Core.Enums;

public enum LcdControl
{
    BgWindowEnable = 0b00000001,
    ObjEnable = 0b00000010,
    ObjSize = 0b00000100,
    BgTileMapArea = 0b00001000,
    BgWindowAddressingMode = 0b00010000,
    WindowEnabled = 0b00100000,
    WindowTileMapArea = 0b01000000,
    LcdEnabled = 0b10000000,
}