namespace GBEmulator.Hardware.Components;

using Core.Models;
using Core.Interfaces;
using Core.Enums;
using Core.Options;

public class Ppu : HardwareComponent, IPpu
{
    private const int OamScanCycles = 80;
    private const int DrawPixelsCycles = 172;
    private const int HBlankCycles = 204;
    private const int VBlankCycles = 456;
    private const int ScreenWidth = 160;
    private const int ScreenHeight = 144;
    private const int VBlankEnd = 153;
    private int[] pixelColours = new int[] { 0x00FFFFFF, 0x00808080, 0x00404040, 0 };
    
    /// <summary>
    /// LCD control register
    /// </summary>
    private byte LCDC
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.LCDC);
        set => _bus.WriteMemory((ushort)HardwareRegisters.LCDC, value);
    }

    /// <summary>
    /// LY register. Indicates the current scanline about to be drawn/being drawn/just been drawn. (0 - 153)
    /// </summary>
    private byte LY
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.LY);
        set => _bus.WriteMemory((ushort)HardwareRegisters.LY, value);
    }

    /// <summary>
    /// LY Compare register. Constantly compared to LY register. If they match, a flag is set in the stat register.
    /// </summary>
    private byte LYC
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.LYC);
        set => _bus.WriteMemory((ushort)HardwareRegisters.LYC, value);
    }

    /// <summary>
    /// LCD Status register. Bits 0-1 contain PPU mode, bit 2 contains LY=LCY flag, bits 3-6 contain interrupt source flags.
    /// </summary>
    private byte STAT
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.STAT);
        set => _bus.WriteMemory((ushort)HardwareRegisters.STAT, value);
    }

    /// <summary>
    /// Current mode PPU is in ( as set by bits 0-1 of STAT register )
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private PpuMode CurrentMode
    {
        get
        {
            return (STAT & 0b11) switch
            {
                (byte)PpuMode.HBlank => PpuMode.HBlank,
                (byte)PpuMode.VBlank => PpuMode.VBlank,
                (byte)PpuMode.OamSearch => PpuMode.OamSearch,
                (byte)PpuMode.DrawingPixels => PpuMode.DrawingPixels,

                // Unreachable
                _ => throw new InvalidOperationException()
            };
        }
        set
        {
            STAT = (byte)((STAT & 0x11111100) | (byte)value);
            if (value is PpuMode.OamSearch && ((1 << 5) & STAT) > 0)
            {
                _bus.Interrupt(Interrupt.LCDCSTATUS);
            } 
            else if (value is PpuMode.HBlank && ((1 << 3) & STAT) > 0)
            {
                _bus.Interrupt(Interrupt.LCDCSTATUS);
            }
            else if (value is PpuMode.VBlank && ((1 << 4) & STAT) > 0)
            {
                _bus.Interrupt(Interrupt.LCDCSTATUS);
            }
        }
    }

    /// <summary>
    /// Bit 2 of STAT register. Represents whether LY=LYC
    /// </summary>
    private bool LycLyFlag
    {
        get => (STAT & 0b00000100) > 0;
        set
        {
            if (value)
                STAT |= 0b00000100;
            else
                STAT &= 0b11111011;
        }
    }

    /// <summary>
    /// Is LCD turned on? ( Bit 7 )
    /// </summary>
    private bool LcdEnabled => GetLcdControlValue(LcdControl.LcdEnabled);

    /// <summary>
    /// Base address for area where tile map data for the window is found (0x9C00 or 0x9800) ( Bit 6 )
    /// </summary>
    private ushort WindowTileMapArea => (ushort)(GetLcdControlValue(LcdControl.WindowTileMapArea) ? 0x9C00 : 0x9800);

    /// <summary>
    /// Is window enabled? ( Bit 5 )
    /// </summary> 
    private bool WindowEnabled => GetLcdControlValue(LcdControl.WindowEnabled);

    /// <summary>
    /// Addressing mode used by BG/Window. If 0x8000, base address is 0x8000 and uses unsigned addressing. If 0x8800, base address is 0x9000 and uses signed addressing.
    /// ( Bit 4 )
    /// </summary>
    private ushort BgWindowAddressingMode =>
        (ushort)(GetLcdControlValue(LcdControl.BgWindowAddressingMode) ? 0x8000 : 0x8800);

    /// <summary>
    /// Base address for area where tile map data for the background is found (0x9C00 or 0x9800) ( Bit 3 )
    /// </summary>
    private ushort BgTileMapArea => (ushort)(GetLcdControlValue(LcdControl.BgTileMapArea) ? 0x9C00 : 0x9800);

    /// <summary>
    /// Are two tile objects enabled? ( Bit 2 )
    /// </summary>
    private int ObjectHeight => GetLcdControlValue(LcdControl.ObjSize) ? 16 : 8;

    /// <summary>
    /// Are objects displayed? ( Bit 1 )
    /// </summary>
    private bool ObjectsEnabled => GetLcdControlValue(LcdControl.ObjEnable);

    /// <summary>
    /// Is the Background/Window displayed? ( Bit 0 )
    /// </summary>
    private bool BgWindowEnabled => GetLcdControlValue(LcdControl.BgWindowEnable);

    /// <summary>
    /// Checks value of flag in LCDC register
    /// </summary>
    /// <param name="control">Flag to check</param>
    /// <returns>Boolean representing flag enabled or not.</returns>
    private bool GetLcdControlValue(LcdControl control)
    {
        return (LCDC & (byte)control) > 0;
    }

    private readonly ILcd _output;

    public Ppu(AppSettings appSettings, ILcd output)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _objsToDraw = new List<Object>();
    }
    
    public override void ConnectToBus(IBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _bus.SetBitmap(_output.Bitmap);
    }

    public void Clock(int numberOfCycles)
    {
        cyclesCompletedThisScanline += numberOfCycles;

        if (!LcdEnabled)
        {
            cyclesCompletedThisScanline = 0;
            LY = 0;
            STAT = (byte)(STAT & ~0b00000011);
            return;
        }

        switch (CurrentMode)
        {
            case PpuMode.OamSearch:
                ScanOam();
                break;
            case PpuMode.DrawingPixels:
                DrawPixels();
                break;
            case PpuMode.HBlank:
                if (cyclesCompletedThisScanline >= HBlankCycles)
                {
                    LY++;
                    cyclesCompletedThisScanline -= HBlankCycles;

                    if (LY == ScreenHeight)
                    {
                        CurrentMode = PpuMode.VBlank;
                        _bus.Interrupt(Interrupt.VBLANK);
                        _bus.FlipWindow();
                    }
                    else
                    {
                        CurrentMode = PpuMode.OamSearch;
                    }
                }

                break;
            case PpuMode.VBlank:
                if (cyclesCompletedThisScanline >= VBlankCycles)
                {
                    LY++;
                    cyclesCompletedThisScanline -= VBlankCycles;

                    if (LY > VBlankEnd)
                    {
                        CurrentMode = PpuMode.OamSearch;
                        LY = 0;
                    }
                }

                break;
            default:
                throw new InvalidOperationException(CurrentMode.ToString());
        }

        if (LY == LYC)
        {
            STAT = (byte)(STAT | 0x00000100);
            if ((STAT & 0b01000000) > 0)
            {
                _bus.Interrupt(Interrupt.LCDCSTATUS);
            }
        }
        else
        {
            STAT = (byte)(STAT & ~0x00000100);
        }
    }

    private bool oamScanComplete = false;
    private int cyclesCompletedThisScanline = 0;
    private List<Object> _objsToDraw;
    private readonly AppSettings _appSettings;

    private void DrawPixels()
    {
        if (cyclesCompletedThisScanline >= DrawPixelsCycles)
        {
            CurrentMode = PpuMode.HBlank;
            cyclesCompletedThisScanline -= DrawPixelsCycles;

            if (BgWindowEnabled)
            {
                DrawBackgroundWindowScanLine();
            }

            if (ObjectsEnabled)
            {
                DrawObjectsOnScanLine();
            }
        }
    }

    private void DrawBackgroundWindowScanLine()
    {
        var WindowX = (byte)(_bus.ReadMemory((ushort)HardwareRegisters.WX) - 7);
        var WindowY = _bus.ReadMemory((ushort)HardwareRegisters.WY);
        var ScrollX = _bus.ReadMemory((ushort)HardwareRegisters.SCX);
        var ScrollY = _bus.ReadMemory((ushort)HardwareRegisters.SCY);
        var BackgroundPalette = _bus.ReadMemory((ushort)HardwareRegisters.BGP);

        // Check if we need to draw part of the window on this line
        var scanlineHasWindow = WindowEnabled && WindowY <= LY;
        var y = scanlineHasWindow ? (byte)(LY - WindowY) : (byte)(LY + ScrollY);

        // One tile is 8x8, so figure out where to put this tile on the screen vertically
        var tileLine = (byte)((y & 0b0111) * 2);
        
        var tileRow = (ushort)(y / 8 * 32);
        
        var baseTileMapAddress = scanlineHasWindow ? WindowTileMapArea : BgTileMapArea;
        
        byte lowByte = 0;
        byte highByte = 0;

        for (var currentPixel = 0; currentPixel < ScreenWidth; currentPixel++)
        {
            var pixelIsWindow = scanlineHasWindow && currentPixel >= WindowX;
            var x = (byte)(currentPixel + ScrollX);
            if (pixelIsWindow)
            {
                x = (byte)(currentPixel - WindowX);
            }

            // if the current pixel is the start of a tile
            if ((currentPixel & 0b0111) == 0 || ((currentPixel + ScrollX) & 0b0111) == 0)
            {
                // One tile is 8x8, so figure out where to put this tile on the screen horizontally
                var tileColumn = (ushort)(x / 8);
                var tileAddress = (ushort)(baseTileMapAddress + tileRow + tileColumn);

                ushort tileLocation;
                if (BgWindowAddressingMode == 0x8000)
                {
                    tileLocation = (ushort) (BgWindowAddressingMode + _bus.ReadMemory(tileAddress) * 16);
                }
                else
                {
                    tileLocation = (ushort) (BgWindowAddressingMode + ((sbyte)_bus.ReadMemory(tileAddress) + 128 ) * 16);
                }

                lowByte = _bus.ReadMemory((ushort)(tileLocation + tileLine));
                highByte = _bus.ReadMemory((ushort)(tileLocation + tileLine + 1));
            }

            var pixelIndex = 7 - (x & 7);
            var highBit = (highByte >> pixelIndex) & 1;
            var lowBit = (lowByte >> pixelIndex) & 1;

            var colour = (highBit << 1) | lowBit;
            var colourAfterApplyingBackgroundPalette = (BackgroundPalette >> colour * 2) & 0b11;

            DrawPixel(currentPixel, LY, pixelColours[colourAfterApplyingBackgroundPalette]);
        }
    }

    private void DrawObjectsOnScanLine()
    {
        foreach (var obj in _objsToDraw)
        {
            var tileRow = (obj.Attributes & 0b01000000) > 0
                ? ObjectHeight - 1 - (LY - obj.YPosition)
                : (LY - obj.YPosition);

            // Objs always use 0x8000 addressing mode
            var tileAddress = 0x8000 + (obj.TileIndex * 16) + (tileRow * 2);

            var lowByte = _bus.ReadMemory((ushort)tileAddress);
            var highByte = _bus.ReadMemory((ushort)(tileAddress + 1));

            var backgroundPalette = _bus.ReadMemory((ushort)HardwareRegisters.BGP);
            var whiteVal = pixelColours[backgroundPalette & 0b11];

            for (var currentPixel = 0; currentPixel < 8; currentPixel++)
            {
                var pixelXPosition = (obj.Attributes & 0b00100000) > 0 ? currentPixel : 7 - currentPixel;
                var pixelColour = (((highByte >> pixelXPosition) & 1) << 1) | ((lowByte >> pixelXPosition) & 1);
                if ((obj.XPosition + currentPixel) >= 0 && (obj.XPosition + currentPixel) < ScreenWidth)
                {
                    // (7th bit of obj attribute: 0 => Object is above background 1=> Object is behind background) || Background is white
                    if (pixelColour != 0 && ((obj.Attributes & 0b10000000) == 0 || _output.GetPixel(pixelXPosition + currentPixel, LY) == whiteVal)) //
                    {
                        DrawPixel(currentPixel + obj.XPosition, LY, pixelColours[pixelColour]);
                    }
                }
            }
        }
    }

    private void DrawPixel(int x, int y, int colour)
    {
        for (var j = 0; j < _appSettings.Scale; j++)
        {
            for (var i = 0; i < _appSettings.Scale; i++)
            {
                _output.SetPixel(x * _appSettings.Scale + i, y * _appSettings.Scale + j, colour);
            }
        }
    }

    private void ScanOam()
    {
        if (oamScanComplete)
        {
            if (cyclesCompletedThisScanline >= OamScanCycles)
            {
                CurrentMode = PpuMode.DrawingPixels;
                oamScanComplete = false;
                cyclesCompletedThisScanline -= OamScanCycles;
            }

            return;
        }

        _objsToDraw = new List<Object>();
        // Iterate over object Y positions
        for (ushort i = 0xFE9C; i >= 0xFE00; i -= 4)
        {
            var objYPos = _bus.ReadMemory(i) - 16;
            if ((objYPos <= LY && LY < (objYPos + ObjectHeight)) && _objsToDraw.Count < 10)
            {
                _objsToDraw.Add(new Object()
                {
                    YPosition = objYPos,
                    XPosition = _bus.ReadMemory((ushort)(i + 1)) - 8,
                    TileIndex = _bus.ReadMemory((ushort)(i + 2)),
                    Attributes = _bus.ReadMemory((ushort)(i + 3)),
                });
            }
        }

        oamScanComplete = true;
    }
}