using System.Drawing;
using System.Linq.Expressions;

namespace GBEmulator.Hardware;

using Core.Models;
using Core.Interfaces;
using Core.Enums;

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
        set => STAT = (byte)((STAT & 0x11111100) | (byte) value);
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
    /// Is LCD turned on?
    /// </summary>
    private bool LcdEnabled => GetLcdControlValue(LcdControl.LcdEnabled);

    /// <summary>
    /// Base address for area where tile map data for the window is found (0x9C00 or 0x9800)
    /// </summary>
    private ushort WindowTileMapArea => (ushort)(GetLcdControlValue(LcdControl.WindowTileMapArea) ? 0x9C00 : 0x9800);

    /// <summary>
    /// Is window enabled?
    /// </summary>
    private bool WindowEnabled => GetLcdControlValue(LcdControl.WindowEnabled);

    /// <summary>
    /// Addressing mode used by BG/Window. If 0x8000, base address is 0x8000 and uses unsigned addressing. If 0x8800, base address is 0x9000 and uses signed addressing.
    /// </summary>
    private ushort BgWindowAddressingMode =>
        (ushort)(GetLcdControlValue(LcdControl.BgWindowAddressingMode) ? 0x8800 : 0x8000);

    /// <summary>
    /// Base address for area where tile map data for the background is found (0x9C00 or 0x9800)
    /// </summary>
    private ushort BgTileMapArea => (ushort)(GetLcdControlValue(LcdControl.BgTileMapArea) ? 0x9C00 : 0x9800);

    /// <summary>
    /// Are two tile objects enabled?
    /// </summary>
    private bool LargeObjects => GetLcdControlValue(LcdControl.ObjSize);

    /// <summary>
    /// Are objects displayed?
    /// </summary>
    private bool ObjectsEnabled => GetLcdControlValue(LcdControl.ObjEnable);

    /// <summary>
    /// Is the Background/Window displayed?
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

    private Lcd Output { get; set; }

    public Ppu()
    {
        Output = new Lcd();
    }

    public void Clock(int numberOfCycles)
    {
        //if (!LcdEnabled)
        //{
        //    cyclesCompletedThisScanline = 0;
        //    LY = 0;
        //    STAT = (byte)(STAT & ~0b00000011);
        //    return;
        //}
        
        cyclesCompletedThisScanline += numberOfCycles;

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
                        _bus.FlipWindow(Output.Bitmap);
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
    }

    private bool oamScanComplete = false;
    private int cyclesCompletedThisScanline = 0;

    private void DrawPixels()
    {
        if (cyclesCompletedThisScanline >= DrawPixelsCycles)
        {
            CurrentMode = PpuMode.HBlank;
            cyclesCompletedThisScanline -= DrawPixelsCycles;
        }
        
        if (BgWindowEnabled)
        {
            DrawBackgroundWindowScanLine();
        }

        if (ObjectsEnabled)
        {
            //DrawObjects();
        }
    }

    private void DrawBackgroundWindowScanLine()
    {
        var WindowX = _bus.ReadMemory((ushort)HardwareRegisters.WX) - 7; 
        var WindowY = _bus.ReadMemory((ushort)HardwareRegisters.WY);
        var ScrollX = _bus.ReadMemory((ushort)HardwareRegisters.SCX);
        var ScrollY = _bus.ReadMemory((ushort)HardwareRegisters.SCY);
        var BackgroundPallette = _bus.ReadMemory((ushort)HardwareRegisters.BGP);
        
        // Check if we need to draw part of the window on this line
        var scanlineHasWindow = WindowEnabled && WindowY <= LY;
        var y = scanlineHasWindow ? LY - WindowY : LY + ScrollY;

        // One tile is 8x8, so figure out where to put this tile on the screen vertically
        var tileRow = y / (8 * 32);
        
        
        var tileLine = (y & 0b0111) * 2;

        var baseTileMapAddress = scanlineHasWindow ? WindowTileMapArea : BgTileMapArea;
        var lowByte = 0;
        var highByte = 0;
        
        for (var currentPixel = 0; currentPixel < ScreenWidth; currentPixel++)
        {
            var pixelIsWindow = scanlineHasWindow && currentPixel >= WindowX;
            var x = currentPixel + ScrollX;
            if (pixelIsWindow)
            {
                x = currentPixel - WindowX;
            }
            
            // if the current pixel is the start of a tile
            if ((currentPixel & 0b0111) == 0 || ((currentPixel + ScrollX) & 0b01111) == 0)
            {
                // One tile is 8x8, so figure out where to put this tile on the screen horizontally
                var tileColumn = x / 8;
                var tileLocation = baseTileMapAddress + tileRow + tileColumn;

                var l = (BgWindowAddressingMode + (BgWindowAddressingMode is 0x8800 ? ((sbyte)_bus.ReadMemory((ushort)tileLocation) + 128) : _bus.ReadMemory((ushort)tileLocation))) * 16;

                lowByte = _bus.ReadMemory((ushort)(l + tileLine));
                highByte = _bus.ReadMemory((ushort)(l + tileLine + 1));
            }

            var pixelIndex = 7 - (x & 7);
            var highBit = (highByte >> pixelIndex) & 1;
            var lowBit = (lowByte >> pixelIndex) & 1;

            var colour = (highBit << 1) | lowBit;
            
            Output.SetPixel(currentPixel, LY, pixelColours[colour]);
        }
        
        
    }

    private void ScanOam()
    {
        if (oamScanComplete)
        {
            if (cyclesCompletedThisScanline > OamScanCycles)
            {
                CurrentMode = PpuMode.DrawingPixels;
                oamScanComplete = false;
                cyclesCompletedThisScanline -= OamScanCycles;
            }

            return;
        }
        var objsToDraw = new List<Object>();
        // Iterate over object Y positions
        for (ushort i = 0x8000; i < 0x8FFF; i+=4)
        {
            if (i == LY && objsToDraw.Count < 10)
            {
                objsToDraw.Add(new Object()
                {
                    YPosition = _bus.ReadMemory(i),
                    XPosition = _bus.ReadMemory((ushort)(i + 1)),
                    TileIndex = _bus.ReadMemory((ushort)(i + 2)),
                    Attributes = _bus.ReadMemory((ushort)(i + 3)),
                });
            }
        }

        oamScanComplete = true;
    }
}