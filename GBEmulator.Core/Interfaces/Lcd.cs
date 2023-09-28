using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GBEmulator.Core.Options;

namespace GBEmulator.Core.Interfaces;

public class Lcd : IDisposable
{
    public Bitmap Bitmap { get; private set; }
    public Int32[] Bits { get; private set; }
    public bool Disposed { get; private set; }
    public static int Height = 144;
    public static int Width = 160;

    private readonly int _calculatedHeight;
    private readonly int _calculatedWidth;
    private readonly AppSettings _appSettings;

    protected GCHandle BitsHandle { get; private set; }

    public Lcd(AppSettings appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _calculatedHeight = Height * _appSettings.Scale;
        _calculatedWidth = Width * _appSettings.Scale;
        Bits = new int[_calculatedWidth * _calculatedHeight];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        Bitmap = new Bitmap(_calculatedWidth, _calculatedHeight, _calculatedWidth * 4, PixelFormat.Format32bppRgb, BitsHandle.AddrOfPinnedObject());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(int x, int y, int colour)
    {
        try
        {
            var index = x + (y * _calculatedWidth);
            Bits[index] = colour;
        }
        catch
        {
            // Ignored
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPixel(int x, int y)
    {
        var index = x  + (y * _calculatedWidth);
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