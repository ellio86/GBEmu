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
        // Force Console
        //AllocConsole();
        _gameBoy = new GameBoy(this);
        _gameBoy.Initialise();
    }


    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();
}
