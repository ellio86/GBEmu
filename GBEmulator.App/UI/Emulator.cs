namespace GBEmulator.App.UI;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public partial class Emulator : Form
{
    private GameBoy _gameBoy;
    
    public Emulator()
    {
        InitializeComponent();
    }

    private void Emulator_Load(object sender, EventArgs e)
    {
        _gameBoy = new GameBoy(this);
        _gameBoy.Initialise();
        
        // Force Console
        //AllocConsole();
    }
    
    private void Emulator_KeyDown(object sender, KeyEventArgs e) {
       _gameBoy.GamePad.HandleKeyDown(GetKeyBit(e));
    }

    private void Emulator_KeyUp(object sender, KeyEventArgs e) {
         _gameBoy.GamePad.HandleKeyUp(GetKeyBit(e));
    }
    
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();
    
    private byte GetKeyBit(KeyEventArgs e) {
        switch (e.KeyCode) {
            case Keys.D:
            case Keys.Right:
                return 0x11;

            case Keys.A:
            case Keys.Left:
                return 0x12;

            case Keys.W:
            case Keys.Up:
                return 0x14;

            case Keys.S:
            case Keys.Down:
                return 0x18;

            case Keys.J:
            case Keys.Z:
                return 0x21;

            case Keys.K:
            case Keys.X:
                return 0x22;

            case Keys.Space:
            case Keys.C:
                return 0x24;

            case Keys.Enter:
            case Keys.V:
                return 0x28;
        }
        return 0;
    }
}
