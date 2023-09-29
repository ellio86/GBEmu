namespace GBEmulator.App;
using Core.Interfaces;
public class ImageControl : IImageControl
{
    private readonly Form _window;
    private readonly PictureBox _pictureBox;
    public ImageControl(Form window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _pictureBox = new PictureBox();
    }
    public void Flip()
    {
        _pictureBox?.Invalidate();
    }

    /// <summary>
    /// Thread safe method for 
    /// </summary>
    /// <param name="bmp"></param>
    public void SetBitmap(Bitmap bmp)
    {
        try
        {
            if (_window.InvokeRequired)
            {
                Action setBitmap = delegate { SetBitmap(bmp); };
                _window.Invoke(setBitmap);

            }
            else
            {
                _pictureBox.Image = bmp;
                _pictureBox.Size = new Size(bmp.Width, bmp.Height);
                _window.Controls.Add(_pictureBox);
            }
        }
        catch
        {
            // ignored
        }

    }
}