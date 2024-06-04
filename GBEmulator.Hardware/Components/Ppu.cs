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
    private const int VBlankCycles = 4560;
    private const int ScreenWidth = 160;
    private const int ScreenHeight = 144;
    private const int VBlankEnd = 153;
    private int[] pixelColours = new int[] { 0x00FFFFFF, 0x00808080, 0x00404040, 0 };
    
    /// <summary>
    /// LCD control register
    /// </summary>
    private byte LCDC
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.LCDC, false);
        set => _bus.WriteMemory((ushort)HardwareRegisters.LCDC, value, false);
    }

    /// <summary>
    /// LY register. Indicates the current scanline about to be drawn/being drawn/just been drawn. (0 - 153)
    /// </summary>
    private byte LY
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.LY, false);
        set => _bus.WriteMemory((ushort)HardwareRegisters.LY, value, false);
    }

    /// <summary>
    /// LY Compare register. Constantly compared to LY register. If they match, a flag is set in the stat register.
    /// </summary>
    private byte LYC
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.LYC, false);
        set => _bus.WriteMemory((ushort)HardwareRegisters.LYC, value, false);
    }

    /// <summary>
    /// LCD Status register. Bits 0-1 contain PPU mode, bit 2 contains LY=LCY flag, bits 3-6 contain interrupt source flags.
    /// </summary>
    private byte STAT
    {
        get => _bus.ReadMemory((ushort)HardwareRegisters.STAT, false);
        set => _bus.WriteMemory((ushort)HardwareRegisters.STAT, value, false);
    }

    /// <summary>
    /// Current mode PPU is in ( as set by bits 0-1 of STAT register )
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public PpuMode CurrentMode
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
        private set
        {
            STAT = (byte)((STAT & 0b11111100) | (byte)value);
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

    public Ppu(ILcd output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _objsToDraw = new List<Object>();
    }
    
    public override void ConnectToBus(IBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _bus.SetBitmap(_output.Bitmap);
    }

    private bool _coincidenceForLY = false;

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
                    _coincidenceForLY = false;
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
                    _coincidenceForLY = false;
                    cyclesCompletedThisScanline -= VBlankCycles;

                    if (LY > VBlankEnd)
                    {
                        CurrentMode = PpuMode.OamSearch;
                        LY = 0;
                        _windowInternalLineCounter = 0;
                    }
                }

                break;
            default:
                throw new InvalidOperationException(CurrentMode.ToString());
        }

        if (LY == LYC)
        {

            STAT = (byte)(STAT | 0b00000100);
            if ((STAT & 0b01000000) > 0 && !_coincidenceForLY)
            {
                _coincidenceForLY = true;
                _bus.Interrupt(Interrupt.LCDCSTATUS);
            }
        }
        else
        {
            STAT = (byte)(STAT & ~0b00000100);
        }
    }

    private bool oamScanComplete = false;
    private int cyclesCompletedThisScanline = 0;
    private List<Object> _objsToDraw;
    private int _windowInternalLineCounter;

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
            else
            {
                // Fill background in with white if the Background is disabled
                var backgroundPalette = _bus.ReadMemory((ushort)HardwareRegisters.BGP, false);
                for (var currentPixel = 0; currentPixel < ScreenWidth; currentPixel++)
                {
                    var colour = 0;
                    var colourAfterApplyingBackgroundPalette = (backgroundPalette >> colour * 2) & 0b11;
                    _output.SetPixel(currentPixel, LY, pixelColours[colourAfterApplyingBackgroundPalette]);
                }
            }

            if (ObjectsEnabled)
            {
                DrawObjectsOnScanLine();
            }
        }
    }
    
    private void DrawBackgroundWindowScanLine()
    {
        var wx = _bus.ReadMemory((ushort)HardwareRegisters.WX, false);
        var windowX = wx < 8 ? (byte)0 : (byte)(wx - 7);
        var windowY = _bus.ReadMemory((ushort)HardwareRegisters.WY, false);
        var scrollX = _bus.ReadMemory((ushort)HardwareRegisters.SCX, false);
        var scrollY = _bus.ReadMemory((ushort)HardwareRegisters.SCY, false);
        var backgroundPalette = _bus.ReadMemory((ushort)HardwareRegisters.BGP, false);

        // Check if we need to draw part of the window on this line
        var scanlineHasWindow = WindowEnabled && windowY <= LY;
        var windowIsVisible = windowX is >= 0 and <= 166 && windowY is >= 0 and <= 143;
        var windowEnabled = windowIsVisible && scanlineHasWindow;

        byte lowByte = 0;
        byte highByte = 0;

        // Draw pixels on scanline
        for (var currentPixel = 0; currentPixel < ScreenWidth; currentPixel++)
        {
            var pixelIsWindow = windowEnabled && currentPixel >= windowX;
            
            // Values for drawing BG
            // The sum of both the X-POS+SCX and LY+SCY offsets is ANDed with 0x3ff in order to ensure that the address stays within the Tilemap memory regions.
            var x = (byte)((currentPixel + scrollX) & 0x3FF);
            var y = (byte)((LY + scrollY) & 0x3FF);
            
            var baseTileMapAddress = pixelIsWindow ? WindowTileMapArea : BgTileMapArea;
            
            if (pixelIsWindow)
            {
                y = (byte)(_windowInternalLineCounter);
                x = (byte)(currentPixel - windowX);
            }
            
            // Calculate offsets for the address
            var tileLine = (byte)((y % 8) * 2);
            var tileNumber = (ushort)((y / 8) * 32);

            // if the current pixel is the start of a tile
            if ((currentPixel & 0b0111) == 0 || ((currentPixel + scrollX) & 0b0111) == 0)
            {
                // apply offsets to the address
                var tileColumn = (ushort)(x / 8);
                var tileAddress = (ushort)(baseTileMapAddress + tileNumber + tileColumn);

                ushort tileLocation;
                if (BgWindowAddressingMode == 0x8000)
                {
                    tileLocation = (ushort) (0x8000 + _bus.ReadMemory(tileAddress, false) * 16);
                }
                else
                {
                    tileLocation = (ushort) (0x9000 + (sbyte)_bus.ReadMemory(tileAddress, false) * 16);
                }

                lowByte = _bus.ReadMemory((ushort)(tileLocation + tileLine), false);
                highByte = _bus.ReadMemory((ushort)(tileLocation + tileLine + 1), false);
            }

            var pixelIndex = 7 - (x & 7);
            var highBit = (highByte >> pixelIndex) & 1;
            var lowBit = (lowByte >> pixelIndex) & 1;

            var colour = (highBit << 1 | lowBit);
            var colourAfterApplyingBackgroundPalette = (backgroundPalette >> colour * 2) & 0b11;

            _output.SetPixel(currentPixel, LY, pixelColours[colourAfterApplyingBackgroundPalette]);
        }
        
        // Increase window line counter if a window was rendered on this scanline
        if (windowEnabled)
        {
            _windowInternalLineCounter++;
        }
    }

    private static List<Object> OrderOamObjects(List<Object> objects)
    {
        // We need to order the objects by their descending x position, so that objects with higher x positions get
        // drawn first, then objects with lower x positions get drawn after as they have priority. For objects with the
        // same x position, the one that occurs first in the OAM takes priority
        
        // Clone list
        var tempList = new List<Object>(objects).OrderByDescending(o => o.XPosition).ToList();
        var returnList = new List<Object>(tempList);

        for (var i = 0; i < returnList.Count; i++)
        {
            if (i is not 0)
            {
                var prevObj = tempList[i - 1];
                var currentObj = tempList[i];

                if (prevObj.XPosition == currentObj.XPosition)
                {
                    // if the prev obj occurs in the original OAM before the current object, swap them so that prevObj has priority (is drawn last)
                    if (objects.IndexOf(prevObj) < objects.IndexOf(currentObj))
                    {
                        returnList[i - 1] = currentObj;
                        returnList[i] = prevObj;
                        i++;
                    }
                }
            }
        }

        return returnList;
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

            var lowByte = _bus.ReadMemory((ushort)tileAddress, false);
            var highByte = _bus.ReadMemory((ushort)(tileAddress + 1), false);
            var backgroundPalette = _bus.ReadMemory((ushort)HardwareRegisters.BGP, false);

            var palette = (obj.Attributes & 0b00010000) > 0
                ? _bus.ReadMemory((ushort)HardwareRegisters.OBP1, false)
                : _bus.ReadMemory((ushort)HardwareRegisters.OBP0, false);

            var whiteVal = pixelColours[backgroundPalette & 0b11];

            for (var currentPixel = 0; currentPixel < 8; currentPixel++)
            {
                var pixelXPosition = (obj.Attributes & 0b00100000) > 0 ? currentPixel : 7 - currentPixel;
                var pixelColour = (((highByte >> pixelXPosition) & 1) << 1) | ((lowByte >> pixelXPosition) & 1);
                if ((obj.XPosition + currentPixel) >= 0 && (obj.XPosition + currentPixel) < ScreenWidth)
                {
                    // (7th bit of obj attribute: 0 => Object is above background 1=> Object is behind background) || Background is white
                    if (pixelColour != 0 && ((obj.Attributes & 0b10000000) == 0 || _output.GetPixel(obj.XPosition + currentPixel, LY) == whiteVal)) //
                    {
                        var colourAfterApplyingPalette = (palette >> pixelColour * 2) & 0b11;
                        _output.SetPixel(currentPixel + obj.XPosition, LY, pixelColours[colourAfterApplyingPalette]);
                    }
                }
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
        for (ushort i = 0xFE00; i <= 0xFE9C; i += 4)
        {
            var objYPos = _bus.ReadMemory(i, false) - 16;
            if ((objYPos <= LY && LY < (objYPos + ObjectHeight)) && _objsToDraw.Count < 10)
            {
                _objsToDraw.Add(new Object()
                {
                    YPosition = objYPos,
                    XPosition = _bus.ReadMemory((ushort)(i + 1), false) - 8,
                    TileIndex = ObjectHeight == 16 ? (byte)(_bus.ReadMemory((ushort)(i + 2), false) & 0b11111110) : _bus.ReadMemory((ushort)(i + 2), false),
                    Attributes = _bus.ReadMemory((ushort)(i + 3), false),
                });
            }
        }

        _objsToDraw = OrderOamObjects(_objsToDraw);

        oamScanComplete = true;
    }
}