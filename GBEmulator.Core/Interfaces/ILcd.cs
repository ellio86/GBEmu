namespace GBEmulator.Core.Interfaces;

using System.Drawing;

public interface ILcd
{
    public Bitmap Bitmap { get; }
    public void SetPixel(int x, int y, int colour);
    public int GetPixel(int x, int y);
}