namespace GBEmulator.Core.Interfaces;

using System.Drawing;


public interface IImageControl : IHardwareComponent
{
    /// <summary>
    /// Updates image so that new bmp pixels are displayed
    /// </summary>
    public void Flip();

    /// <summary>
    /// Associates this control with the provided bitmap
    /// </summary>
    public void SetBitmap(Bitmap bmp);
}