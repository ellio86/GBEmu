namespace GBEmulator.Core.Interfaces;

using System.Drawing;

public interface IWindow
{
    public void Flip();
    public void SetBitmap(Bitmap bmp);
}