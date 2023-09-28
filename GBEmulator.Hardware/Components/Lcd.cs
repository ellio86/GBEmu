namespace GBEmulator.Hardware.Components;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Options;
using Core.Interfaces;

public class Lcd : ILcd, IDisposable
{
    public Bitmap Bitmap { get; private set; }
    private int[] Bits { get; set; }
    private bool Disposed { get; set; }
    
    public static readonly int Height = 144;
    public static readonly int Width = 160;

    private readonly int _calculatedWidth;
    private readonly AppSettings _appSettings;

    protected GCHandle BitsHandle { get; private set; }

    public Lcd(AppSettings appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        
        // Calculate width/height based on scale setting
        var calculatedHeight = Height * appSettings.Scale;
        _calculatedWidth = Width * appSettings.Scale;
        
        Bits = new int[_calculatedWidth * calculatedHeight];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        
        // Create bitmap with reference to bits
        Bitmap = new Bitmap(_calculatedWidth, calculatedHeight, _calculatedWidth * 4, PixelFormat.Format32bppRgb, BitsHandle.AddrOfPinnedObject());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(int x, int y, int colour)
    {
        try
        {
            for (var i = 0; i < _appSettings.Scale; i++)
            {
                for (var j = 0; j < _appSettings.Scale; j++)
                {
                    var index = (_appSettings.Scale * x + i) + ((j + y * _appSettings.Scale) * _calculatedWidth);
                    Bits[index] = colour;
                }
            }
        }
        catch
        {
            // Ignored
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPixel(int x, int y)
    {
        var index = (x * _appSettings.Scale) + (_appSettings.Scale * y * _calculatedWidth);
        return Bits[index];
    }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        Bitmap.Dispose();
        BitsHandle.Free();
    }
}