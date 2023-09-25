namespace GBEmulator.App;
using Core.Interfaces;
public class Window : IWindow
{
    private Form _window;
    private PictureBox _pictureBox;
    public Window(Form window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }
    public void Flip(Bitmap bmp)
    {
        _pictureBox?.Dispose();
        
        var newPictureBox = new PictureBox();
        newPictureBox.Image = bmp;
        
        UpdateImage(newPictureBox);
    }
    
    /// <summary>
    /// Thread safe function for writing to the window's text
    /// </summary>
    private void UpdateImage(PictureBox pb)
    {
        try
        {
            if (_window.InvokeRequired)
            {
                Action flipImage = delegate { UpdateImage(pb); };
                _window.Invoke(flipImage);
            }
            else
            {
                _window.Controls.Remove(_pictureBox);
                _window.Controls.Add(pb);
                _pictureBox = pb;
            }
        }
        catch
        {
            // ignored
        }
    }
}